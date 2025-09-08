using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Authorization;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.ViewerRole))]
    public class FeedbackSubmittedController : Controller
    {
        private readonly UrlBuilder _urlBuilder;

        public FeedbackSubmittedController(UrlBuilder urlBuilder)
        {
            _urlBuilder = urlBuilder;
        }

        [HttpGet("/{encodedAccountId}/feedback-submitted", Name = RouteNames.FeedbackAlreadySubmitted)]
        public IActionResult Index(string encodedAccountId)
        {
            ViewBag.EmployerAccountsHomeUrl = _urlBuilder.AccountsLink("AccountsHome", encodedAccountId);
            return View();
        }
    }
}