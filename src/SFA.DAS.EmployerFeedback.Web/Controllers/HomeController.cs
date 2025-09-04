using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerProvideFeedback.Infrastructure;
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
    //TODO replace database calls with calls to outer API
    public class HomeController : Controller
    {
        private readonly IEncodingService _encodingService;
        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _config;
        private readonly IStubAuthenticationService _stubAuthenticationService;
        private readonly IHttpContextAccessor _contextAccessor;

        #region Routes
        public const string ErrorRouteGet = nameof(ErrorRouteGet);
        #endregion


        public HomeController(
            ISessionStorageService sessionService,
            IEncodingService encodingService,
            ILogger<HomeController> logger,
            IConfiguration config,
            IStubAuthenticationService stubAuthenticationService,
            IHttpContextAccessor contextAccessor)
        {
            _sessionService = sessionService;
            _encodingService = encodingService;
            _logger = logger;
            _config = config;
            _contextAccessor = contextAccessor;
            _stubAuthenticationService = stubAuthenticationService;
        }

        [Authorize(Policy = nameof(PolicyNames.ViewerRole))]
        [HttpGet]
        [Route(RoutePrefixPaths.FeedbackRoutePath, Name = RouteNames.Landing_Get_New)]
        public async Task<IActionResult> Index(StartFeedbackRequest request)
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);   //System.Security.Claims.ClaimTypes.NameIdentifier
            var sessionSurvey = await _sessionService.Get<SurveyModel>(idClaim.Value);
            if (sessionSurvey == null)
            {
                return NotFound();
            }
            else
            {
                ViewData.Add("ProviderName", sessionSurvey.ProviderName);
            }

            return View();
        }
        
        [Authorize(Policy = nameof(PolicyNames.IsAuthenticated))]
        [ServiceFilter(typeof(EnsureFeedbackNotSubmitted))]
        [Route(RoutePrefixPaths.FeedbackFromEmailRoutePath, Name = RouteNames.Landing_Get)]
        [HttpGet]
        public async Task<IActionResult> Index(Guid uniqueCode)
        {
            //FIXME - Replace call with outer API
            //var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);    //System.Security.Claims.ClaimTypes.NameIdentifier

            //var employerEmailDetail = await _employerEmailDetailsRepository.GetEmployerInviteForUniqueCode(uniqueCode);

            //_logger.LogWarning("Landing Page GET hit");

            //if (employerEmailDetail == null)
            //{
            //    _logger.LogWarning($"Attempt to use invalid unique code: {uniqueCode}");
            //    return NotFound();
            //}

            //FIXME - Replace call with outer API - var providerAttributes = await _employerEmailDetailsRepository.GetAllAttributes();
            //if (providerAttributes == null)
            //{
            //    _logger.LogError($"Unable to load Provider Attributes from the database.");
            //    return RedirectToAction("Error", "Error");
            //}

            //var providerAttributesModel = providerAttributes.Select(s => new ProviderAttributeModel { Name = s.AttributeName });
            //var newSurveyModel = MapToNewSurveyModel(employerEmailDetail, providerAttributesModel);
            //newSurveyModel.UniqueCode = uniqueCode;
            //await _sessionService.Set(idClaim.Value, newSurveyModel);

            //var encodedAccountId = _encodingService.Encode(employerEmailDetail.AccountId, EncodingType.AccountId);
            //return RedirectToRoute(RouteNames.Landing_Get_New, new { encodedAccountId = encodedAccountId });
            throw new NotImplementedException();
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
            var idToken = await HttpContext.GetTokenAsync("id_token");

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
            Response.Cookies.Delete("SFA.DAS.ProvideFeedbackEmployer.Web.Auth");
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
        [Route("SignIn-Stub")]
        public IActionResult SigninStub()
        {
            return View("SigninStub", new List<string>{_config["StubId"],_config["StubEmail"]});
        }
        
        [AllowAnonymous()]
        [HttpPost]
        [Route("SignIn-Stub")]
        public async Task<IActionResult> SigninStubPost()
        {
            var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
            {
                Email = _config["StubEmail"],
                Id = _config["StubId"]
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
                new AuthenticationProperties());

            return RedirectToRoute("Signed-in-stub");
        }

        [Authorize()]
        [HttpGet]
        [Route("signed-in-stub", Name = "Signed-in-stub")]
        public IActionResult SignedInStub()
        {
            return View();
        }
#endif

        private SurveyModel MapToNewSurveyModel(EmployerSurveyInvite employerEmailDetail, IEnumerable<ProviderAttributeModel> providerAttributes)
        {
            return new SurveyModel
            {
                AccountId = employerEmailDetail.AccountId,
                Ukprn = employerEmailDetail.Ukprn,
                UserRef = employerEmailDetail.UserRef,
                Submitted = employerEmailDetail.CodeBurntDate != null,
                ProviderName = employerEmailDetail.ProviderName,
                Attributes = providerAttributes.ToList()
            };
        }
    }
}