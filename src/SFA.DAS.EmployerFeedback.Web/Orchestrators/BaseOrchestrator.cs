using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using System;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IUserService _userService;

        public BaseOrchestrator(IUserService userService)
        {
            _userService = userService;
        }

        public Guid GetCurrentUserId => Guid.Parse(_userService.GetUserId());
    }
}
