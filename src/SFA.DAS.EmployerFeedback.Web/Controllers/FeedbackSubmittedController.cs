using ESFA.DAS.EmployerFeedback.Web.Authentication;
using ESFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Shared.UI;

namespace ESFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
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