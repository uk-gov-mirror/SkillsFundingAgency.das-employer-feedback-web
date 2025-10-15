using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.GovUK.Auth.Employer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployerClaims = SFA.DAS.EmployerFeedback.Infrastructure.Configuration.EmployerClaims;

namespace SFA.DAS.EmployerFeedback.Web.Authorization
{
    public interface IEmployerAccountAuthorisationHandler
    {
        bool IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    }

    public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>, IEmployerAccountAuthorisationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGovAuthEmployerAccountService _accountsService;
        private readonly ILogger<EmployerAccountAuthorizationHandler> _logger;
        private readonly EmployerFeedbackWebConfiguration _configuration;

        public EmployerAccountAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IGovAuthEmployerAccountService accountsService, ILogger<EmployerAccountAuthorizationHandler> logger, IOptions<EmployerFeedbackWebConfiguration> configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _accountsService = accountsService;
            _logger = logger;
            _configuration = configuration.Value;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
        {
            if (!IsEmployerAuthorised(context, false))
            {
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        public bool IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
        {
            if (!_httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey(RouteValueKeys.EncodedAccountId))
            {
                return false;
            }

            var accountIdFromUrl = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.EncodedAccountId].ToString().ToUpper();
            var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AssociatedAccounts));
            
            if (employerAccountClaim?.Value == null)
                return false;

            Dictionary<string, EmployerUserAccountItem> employerAccounts;
            try
            {
                employerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value);
            }
            catch (JsonSerializationException e)
            {
                _logger.LogError(e, "Could not deserialize employer account claim for user");
                return false;
            }

            EmployerUserAccountItem employerIdentifier = null;
            if (employerAccounts != null)
            {
                employerIdentifier = employerAccounts.ContainsKey(accountIdFromUrl)
                    ? employerAccounts[accountIdFromUrl] : null;
            }

            if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
            {
                var requiredIdClaim = ClaimTypes.NameIdentifier;

                if (!context.User.HasClaim(c => c.Type.Equals(requiredIdClaim)))
                    return false;

                var userClaim = context.User.Claims
                    .First(c => c.Type.Equals(requiredIdClaim));

                var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
                var userId = userClaim.Value;
                var result = _accountsService.GetUserAccounts(userId, email).Result;

                var accountsAsJson = JsonConvert.SerializeObject(result.EmployerAccounts.ToDictionary(k => k.AccountId));
                var associatedAccountsClaim = new Claim(EmployerClaims.AssociatedAccounts, accountsAsJson, JsonClaimValueTypes.Json);

                var updatedEmployerAccounts = JsonConvert.DeserializeObject<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);
                userClaim.Subject.AddClaim(associatedAccountsClaim);

                if (!updatedEmployerAccounts.ContainsKey(accountIdFromUrl))
                {
                    return false;
                }
                employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
            }

            if (!_httpContextAccessor.HttpContext.Items.ContainsKey("Employer"))
            {
                _httpContextAccessor.HttpContext.Items.Add("Employer", employerAccounts.GetValueOrDefault(accountIdFromUrl));
            }

            if (!CheckUserRoleForAccess(employerIdentifier, allowAllUserRoles))
            {
                return false;
            }

            return true;
        }
        private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, bool allowAllUserRoles)
        {
            if (!Enum.TryParse<UserRole>(employerIdentifier.Role, true, out var userRole))
            {
                return false;
            }

            return allowAllUserRoles || userRole == UserRole.Owner;
        }
    }
}