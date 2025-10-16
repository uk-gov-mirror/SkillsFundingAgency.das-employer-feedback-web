using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public class EnsureSessionExists : ActionFilterAttribute
    {
        private readonly ISessionStorageService _sessionService;
        private readonly IUserService _userService;
        private readonly ILogger<EnsureSessionExists> _logger;

        public EnsureSessionExists(ISessionStorageService sessionService, ILogger<EnsureSessionExists> logger, IUserService userService)
        {
            _sessionService = sessionService;
            _logger = logger;
            _userService = userService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var c = context.Controller as Controller;
            var userId = _userService.GetUserId();
            var sessionExists = _sessionService.ExistsAsync(userId).Result;

            if (!sessionExists)
            {
                _logger.LogWarning($"Session for user id {userId} does not exist");
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(RouteNames.Landing_Get);
            }
        }
    }
}
