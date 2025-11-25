using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Controllers;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EnsureSessionExistsAttribute : ActionFilterAttribute
    {
        private readonly ISessionStorageService _sessionService;
        private readonly IUserService _userService;
        private readonly ILogger<EnsureSessionExistsAttribute> _logger;

        public EnsureSessionExistsAttribute(ISessionStorageService sessionService, ILogger<EnsureSessionExistsAttribute> logger, IUserService userService)
        {
            _sessionService = sessionService;
            _logger = logger;
            _userService = userService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue || await _sessionService.GetSurveyModel(userId.Value) == null)
            {
                _logger.LogWarning("No survey was started for user id {UserId} or session has timed out", userId.GetValueOrDefault());
                
                var controller = context.Controller as Controller;
                context.Result = controller.RedirectToRoute(ProviderController.ProviderSearchGet);
                return;
            }

            await next();
        }
    }
}
