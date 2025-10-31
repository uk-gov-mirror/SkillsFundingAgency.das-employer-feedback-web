using SFA.DAS.EmployerFeedback.Domain.Types;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization
{
    public interface IEmployerRoleAuthorizationService
    {
        Task<bool> IsEmployerAuthorized(ClaimsPrincipal user, UserRole minimumAllowedRole);
    }
}