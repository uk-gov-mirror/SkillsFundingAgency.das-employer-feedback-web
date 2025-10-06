using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecentlyAttribute : ActionFilterAttribute
    {
        private readonly EmployerFeedbackWebConfiguration _config;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        public EnsureFeedbackNotSubmittedRecentlyAttribute(IEmployerFeedbackOuterApi employerFeedbackOuterApi, EmployerFeedbackWebConfiguration config)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            if (context.ActionArguments.ContainsKey("userref") && context.ActionArguments.ContainsKey("accountId") && context.ActionArguments.ContainsKey("providerId"))
            {
                var providerFeedback = _employerFeedbackOuterApi
                    .GetTrainingProviderSearch((long)context.ActionArguments["accountId"], (Guid)context.ActionArguments["userref"])
                    .GetAwaiter()
                    .GetResult()
                    .Providers
                    .Find(x => x.Ukprn == (long)context.ActionArguments["providerId"]);

                if (providerFeedback.HasCompleted)
                {
                    var controller = context.Controller as Controller;
                    context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
                }
            }
        }
    }
}
