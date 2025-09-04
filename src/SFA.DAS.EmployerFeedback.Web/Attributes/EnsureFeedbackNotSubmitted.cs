using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmitted : ActionFilterAttribute
    {
        //FIXME - Replace call with outer API
        //private readonly IEmployerFeedbackRepository _employerEmailDetailRepository;
        private readonly IEncodingService _encodingService;

        public EnsureFeedbackNotSubmitted(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            //var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(uniqueCode).Result;
            //if (isCodeBurnt)
            //{
            //    var employerEmailDetail = _employerEmailDetailRepository.GetEmployerInviteForUniqueCode(uniqueCode).GetAwaiter().GetResult();
            //    var encodedAccountId = _encodingService.Encode(employerEmailDetail.AccountId, EncodingType.AccountId);
            //    var controller = context.Controller as Controller;
            //    context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = encodedAccountId });
            //}
        }
    }
}
