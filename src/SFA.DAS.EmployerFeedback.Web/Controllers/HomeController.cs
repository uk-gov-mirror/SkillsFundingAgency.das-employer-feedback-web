using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.EmployerFeedback.Web.Models;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.EmployerFeedback.Web.StartupExtensions;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [HideAccountNavigation(true)]
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly UrlBuilder _urlBuilder;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<HomeController> _logger;
        
        //private readonly IStubAuthenticationService _stubAuthenticationService;

        #region Routes
        public const string DashboardStubRouteGet = nameof(DashboardStubRouteGet);
        public const string OverviewStubRouteGet = nameof(OverviewStubRouteGet);
        public const string ErrorRouteGet = nameof(ErrorRouteGet);
        #endregion Routes

        public HomeController(IConfiguration config, UrlBuilder urlBuilder, 
            IHttpContextAccessor contextAccessor, ILogger<HomeController> logger)
        {
            _config = config;
            _urlBuilder = urlBuilder;
            _contextAccessor = contextAccessor;
            _logger = logger;
            //_stubAuthenticationService = stubAuthenticationService;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Index called");
            if (_config.IsRunningLocally() || _config.IsRunningInDev())
            {
                return View();
            }

            return Redirect(_urlBuilder.AccountsLink());
        }

        [HttpGet("~/error/403")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Route("error", Name = ErrorRouteGet)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string errorMessage)
        {
            _logger.LogError(errorMessage);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? _contextAccessor.HttpContext.TraceIdentifier, ErrorMessage = errorMessage });
        }

        [Route("signout", Name = "signout")]
        public new async Task<IActionResult> SignOut()
        {
            var idToken = await _contextAccessor.HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = string.Empty,
                AllowRefresh = true
            };
            
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);

            List<string> authenticationSchemes = new List<string> { CookieAuthenticationDefaults.AuthenticationScheme };
            if (!bool.TryParse(_config["StubAuth"], out bool stubAuth) || !stubAuth)
                authenticationSchemes.Add(OpenIdConnectDefaults.AuthenticationScheme);

            return SignOut(
                authenticationProperties,
                authenticationSchemes.ToArray());
        }

        [Route("signoutcleanup")]
        public void SignOutCleanup()
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete("SFA.DAS.EmployerFeedback.Web.Auth");
        }
#if DEBUG
        //[AllowAnonymous()]
        //[Route("Dashboard-Stub", Name = DashboardStubRouteGet)]
        //public IActionResult DashboardStub()
        //{
        //    _contextAccessor.HttpContext.Response.Cookies.Delete(GovUkConstants.StubAuthCookieName);
        //    return RedirectToRoute(EmployerRequestController.DashboardRouteGet, new { hashedAccountId = SignedInStubViewModel.HashedAccountIdPlaceholder });
        //}


        //[AllowAnonymous()]
        //[Route("Overview-Stub", Name= OverviewStubRouteGet)]
        //public IActionResult OverviewStub()
        //{
        //    _contextAccessor.HttpContext.Response.Cookies.Delete(GovUkConstants.StubAuthCookieName);
        //    return RedirectToRoute(EmployerRequestController.OverviewEmployerRequestRouteGet, new { hashedAccountId = SignedInStubViewModel.HashedAccountIdPlaceholder, standardId=274, requestType=RequestType.Providers });
        //}

        [AllowAnonymous()]
        [HttpGet]
        [Route("SignIn-Stub", Name = "SignInStub")]
        public IActionResult SigninStub(string returnUrl)
        {
            var model = new SignInStubViewModel
            {
                StubId = _config["StubId"],
                StubEmail = _config["StubEmail"],
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        [AllowAnonymous()]
        [HttpPost]
        [Route("SignIn-Stub")]
        public async Task<IActionResult> SigninStubPost(SignInStubViewModel model)
        {
            //var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
            //{
            //    Email = model.StubEmail,
            //    Id = model.StubId
            //});

            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
            //    new AuthenticationProperties());

            return RedirectToRoute("SignedInStub", new { model.ReturnUrl});
        }

        [Authorize()]
        [HttpGet]
        [Route("signed-in-stub", Name = "SignedInStub")]
        public IActionResult SignedInStub(string returnUrl )
        {
            return View(new SignedInStubViewModel(_contextAccessor, returnUrl));
        }
#endif
    }
}