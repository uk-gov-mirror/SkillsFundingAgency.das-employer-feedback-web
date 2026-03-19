using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EmployerClaims = SFA.DAS.EmployerFeedback.Infrastructure.Configuration.EmployerClaims;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetUserId()
        {
            return Guid.TryParse(GetUserClaimAsString(EmployerClaims.UserId), out Guid userId) ? userId : null;
        }

        private bool IsUserAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }

        private bool TryGetUserClaimValue(string key, out string value)
        {
            value = null;

            var claimsIdentity = _httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return false;

            }
            var claim = claimsIdentity.FindFirst(key);
            if (claim == null) {
                return false;
            }
            value = claim.Value;

            return true;
        }

        private string GetUserClaimAsString(string claim)
        {
            if (IsUserAuthenticated() && TryGetUserClaimValue(claim, out var value))
            {
                return value;
            }
            return null;
        }
    }
}