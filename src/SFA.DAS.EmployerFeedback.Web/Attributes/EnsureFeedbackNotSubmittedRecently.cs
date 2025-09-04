using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;

namespace ESFA.DAS.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecentlyAttribute : ActionFilterAttribute
    {
        private readonly EmployerFeedbackWebConfiguration _config;

        public EnsureFeedbackNotSubmittedRecentlyAttribute(EmployerFeedbackWebConfiguration config)
        {
            _config = config;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //FIXME - Replace call with outer API

            //if(context.ActionArguments.ContainsKey("uniqueCode"))
            //{
            //    var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            //    var isCodeBurnt = _employerEmailDetailRepository.IsCodeBurnt(uniqueCode).Result;
            //    if (isCodeBurnt)
            //    {
            //        var dateCodeBurnt = _employerEmailDetailRepository.GetCodeBurntDate(uniqueCode).GetAwaiter().GetResult();
            //        if (dateCodeBurnt.HasValue)
            //        {
            //            var daysSinceFeedback = DateTime.Now - dateCodeBurnt.Value;
            //            if (daysSinceFeedback.TotalDays > _config.FeedbackWaitPeriodDays)
            //            {
            //                return;
            //            }
            //        }
            //        var controller = context.Controller as Controller;
            //        context.Result = controller.RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
            //    }
            //}
        }
    }
}
