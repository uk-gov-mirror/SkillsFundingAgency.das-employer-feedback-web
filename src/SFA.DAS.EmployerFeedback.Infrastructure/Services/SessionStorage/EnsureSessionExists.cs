using System.Security.Claims;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;



namespace SFA.DAS.EmployerFeedback.Infrastructure
{
    public class EnsureSessionExists : ActionFilterAttribute
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<EnsureSessionExists> _logger;

        public EnsureSessionExists(ISessionStorageService sessionService, ILogger<EnsureSessionExists> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var c = context.Controller as Controller;
            var userId = c.User.FindFirstValue(EmployerClaims.UserId);
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
