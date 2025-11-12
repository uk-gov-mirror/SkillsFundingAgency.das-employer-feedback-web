using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.EmployerFeedback.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Configuration;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UrlBuilder _urlBuilder;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IConfiguration _config;

        #region Routes
        public const string ProvideFeedbackStubRouteGet = nameof(ProvideFeedbackStubRouteGet);
        #endregion Routes

        public HomeController(
            UrlBuilder urlBuilder,
            IConfiguration config,
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger)

        {
            _contextAccessor = contextAccessor;
            _urlBuilder = urlBuilder;
            _config = config;
        }

        [AllowAnonymous]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

        [AllowAnonymous]
        [Route("")]
        public IActionResult Index()
        {
            if (_config.IsRunningLocally() || _config.IsRunningInDev())
            {
                return View();
            }
            return Redirect(_urlBuilder.AccountsLink());
        }

        [AllowAnonymous()]
        [Route("ProvideFeedback-Stub", Name = ProvideFeedbackStubRouteGet)]
        public IActionResult ProvideFeedbackStub()
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete(GovUkConstants.StubAuthCookieName);
            return RedirectToRoute(ProviderController.ProviderSearchGet, new { encodedAccountId = SignedInStubViewModel.HashedAccountIdPlaceholder});
        }
    }
}