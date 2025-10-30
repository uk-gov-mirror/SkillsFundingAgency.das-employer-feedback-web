using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Filters
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
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
            string encodedAccountId = null;

            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.UserId))?.Value;

            if (context.RouteData.Values.TryGetValue(RouteValueKeys.EncodedAccountId, out var routeDataEncodedAccountId))
            {
                encodedAccountId = routeDataEncodedAccountId.ToString();
            }

            return new GaData
            {
                UserId = userId,
                Acc = encodedAccountId
            };
        }
    }
}