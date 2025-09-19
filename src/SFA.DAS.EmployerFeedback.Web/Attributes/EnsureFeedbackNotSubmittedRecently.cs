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
            throw new NotImplementedException();
            //if (context.ActionArguments.ContainsKey("uniqueCode"))
            //{
            //    var uniqueCode = (Guid)context.ActionArguments["uniqueCode"];

            //    var isCodeBurnt = _employerFeedbackOuterApi.IsCodeBurnt(uniqueCode).Result;
            //    if (isCodeBurnt)
            //    {
            //        var dateCodeBurnt = _employerFeedbackOuterApi.GetCodeBurntDate(uniqueCode).GetAwaiter().GetResult();
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
