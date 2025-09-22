using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.Encoding;
using System;
using System.Runtime.InteropServices;

namespace SFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        private readonly IEncodingService _encodingService;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;


        public EnsureFeedbackNotSubmitted(IEmployerFeedbackOuterApi employerFeedbackOuterApi, IEncodingService encodingService)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _encodingService = encodingService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var accountId = (long)context.ActionArguments["accountId"];
            var userref = new Guid((string)context.ActionArguments["userref"]);
            
            
            var employerEmailDetail = _employerFeedbackOuterApi.GetTrainingProviderSearch(accountId, userref).GetAwaiter().GetResult();
            var encodedAccountId = _encodingService.Encode(employerEmailDetail.AccountId, EncodingType.AccountId);
            var controller = context.Controller as Controller;
            context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = encodedAccountId });
        }
    }
}
