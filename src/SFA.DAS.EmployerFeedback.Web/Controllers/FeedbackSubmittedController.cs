using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Services;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.ViewerRole))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class FeedbackSubmittedController : Controller
    {
        #region Routes
        public const string FeedbackAlreadySubmittedGet = nameof(FeedbackAlreadySubmittedGet);
        #endregion

        private readonly IAccountsLinkService _accountsLinkService;

        public FeedbackSubmittedController(IAccountsLinkService accountsLinkService)
        {
            _accountsLinkService = accountsLinkService;
        }

        [HttpGet]
        [Route("feedback-submitted", Name = FeedbackAlreadySubmittedGet)]
        public IActionResult Index(string encodedAccountId)
        {
            ViewBag.EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(encodedAccountId);
            return View();
        }
    }
}