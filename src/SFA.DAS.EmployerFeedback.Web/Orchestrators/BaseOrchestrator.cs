using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Extensions;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ILogger<BaseOrchestrator> _logger;

        public BaseOrchestrator(ILogger<BaseOrchestrator> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        protected Guid GetUserId()
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue)
            {
                _logger.LogError($"User id not found in user claims.");
                throw new InvalidOperationException("User id not found in user claims.");
            }

            return userId.Value;
        }

        protected static async Task<bool> ValidateViewModel<T>(IValidator<T> validator, T viewModel, ModelStateDictionary modelState)
        {
            await validator.ValidateAndAddModelErrorsAsync(viewModel, modelState);
            return modelState.IsValid;
        }
    }
}
