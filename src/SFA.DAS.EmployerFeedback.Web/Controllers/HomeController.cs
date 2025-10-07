using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.Encoding;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEncodingService _encodingService;
        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IStubAuthenticationService _stubAuthenticationService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        #region Routes
        public const string ErrorRouteGet = nameof(ErrorRouteGet);
        #endregion


        public HomeController(
            ISessionStorageService sessionService,
            IEncodingService encodingService,
            ILogger<HomeController> logger,
            IConfiguration config,
            IStubAuthenticationService stubAuthenticationService,
            IHttpContextAccessor contextAccessor,
            IEmployerFeedbackOuterApi employerFeedbackOuterApi)
        {
            _sessionService = sessionService;
            _encodingService = encodingService;
            _logger = logger;
            _config = config;
            _contextAccessor = contextAccessor;
            _stubAuthenticationService = stubAuthenticationService;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
        }


        [Authorize(Policy = nameof(PolicyNames.NoneRole))]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index called");
            var surveyModel = await _sessionService.Get<SurveyModel>(User.FindFirst(EmployerClaims.UserId).Value);
           
            if (surveyModel == null)
            {
                return NotFound();
            }

            ViewData.Add("ProviderName", surveyModel.ProviderName);
            return View();
        }


        [Route("error", Name = ErrorRouteGet)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string errorMessage)
        {
            _logger.LogError(errorMessage);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? _contextAccessor.HttpContext.TraceIdentifier, ErrorMessage = errorMessage });
        }

        [Route("signout", Name = RouteNames.Signout)]
        public new async Task<IActionResult> SignOut()
        {
            var idToken = await _contextAccessor.HttpContext.GetTokenAsync("id_token");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token",idToken);
            var schemes = new List<string>
            {
                CookieAuthenticationDefaults.AuthenticationScheme
            };
            _ = bool.TryParse(_config["StubAuth"], out var stubAuth);
            if (!stubAuth)
            {
                schemes.Add(OpenIdConnectDefaults.AuthenticationScheme);
            }
            
            return SignOut(authenticationProperties, schemes.ToArray());
        }

        [AllowAnonymous]
        [Route("signoutcleanup")]
        public void SignOutCleanup()
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete("SFA.DAS.EmployerFeedback.Web.Auth");
        }

        [AllowAnonymous]
        [Route("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }

#if DEBUG
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
            var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
            {
                Email = model.StubEmail,
                Id = model.StubId
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
                new AuthenticationProperties());

            return RedirectToRoute("SignedInStub", new { model.ReturnUrl });
        }

        [Authorize()]
        [HttpGet]
        [Route("signed-in-stub", Name = "SignedInStub")]
        public IActionResult SignedInStub(string returnUrl)
        {
            return View(new SignedInStubViewModel(_contextAccessor, returnUrl));
        }
#endif

        private SurveyModel MapToNewSurveyModel(GetProviderFeedback employerEmailDetail, IEnumerable<Models.Shared.ProviderAttributeModel> providerAttributes)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var sessionSurvey = _sessionService.Get<SurveyModel>(idClaim.Value);
            if (sessionSurvey == null)
            {
                return null;
            }

            return new SurveyModel
            {
                AccountId = employerEmailDetail.AccountId,
                Ukprn = employerEmailDetail.Providers[0].Ukprn,
                UserRef = new Guid(idClaim?.Value),
                Submitted = false,
                ProviderName = employerEmailDetail.Providers[0].ProviderName,
                Attributes = providerAttributes.ToList()
            };
        }
    }
}