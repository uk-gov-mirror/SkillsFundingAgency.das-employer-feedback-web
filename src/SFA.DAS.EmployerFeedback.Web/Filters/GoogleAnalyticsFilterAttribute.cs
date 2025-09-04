using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SFA.DAS.EmployerFeedback.Web.Filters
{
    [ExcludeFromCodeCoverage]
    public class GoogleAnalyticsFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is not Controller controller)
            {
                return;
            }

            controller.ViewBag.GaData = PopulateGaData(context);

            base.OnActionExecuting(context);
        }

        private static GaData PopulateGaData(ActionExecutingContext context)
        {
            string EncodedAccountId = null;

            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.UserId))?.Value;

            if (context.RouteData.Values.TryGetValue("AccountHashedId", out var accountHashedId))
            {
                EncodedAccountId = accountHashedId.ToString();
            }

            return new GaData
            {
                UserId = userId,
                Acc = EncodedAccountId
            };
        }
    }
}