using Microsoft.AspNetCore.Authorization;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Authorization
{
    public class NoneRoleAuthorizationHandler : AuthorizationHandler<NoneRoleRequirement>
    {
        private readonly IEmployerRoleAuthorizationService _employerRoleAuthorizationService;

        public NoneRoleAuthorizationHandler(IEmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            _employerRoleAuthorizationService = employerRoleAuthorizationService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NoneRoleRequirement requirement)
        {
            var isAuthorized = await _employerRoleAuthorizationService.IsEmployerAuthorized(context.User, UserRole.None);

            if (isAuthorized)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }
}