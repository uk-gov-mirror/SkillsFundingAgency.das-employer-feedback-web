using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using System;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    public class ControllerBase : Controller
    {
        internal readonly IUserService _userService;
        private readonly ILogger<ControllerBase> _logger;

        public ControllerBase(IUserService userService, ILogger<ControllerBase> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        public Guid GetUserId()
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogError($"User id not found in user claims.");
                throw new InvalidOperationException("User id not found in user claims.");
            }

            return userId.Value;
        }
    }
}
