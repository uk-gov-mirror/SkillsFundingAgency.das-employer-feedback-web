using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Route("service")]
    public class ServiceController : Controller
    {
        #region Routes;
        public const string SignoutGet = nameof(SignoutGet);
        public const string SignInStubGet = nameof(SignInStubGet);
        public const string SignInStubPost = nameof(SignInStubPost);
        public const string SignedInStubGet = nameof(SignedInStubGet);
        #endregion

        private readonly IConfiguration _config;
        private readonly IStubAuthenticationService _stubAuthenticationService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ISessionService _sessionService;
        private readonly IUserService _userService;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            IConfiguration config,
            IStubAuthenticationService stubAuthenticationService,
            IHttpContextAccessor contextAccessor,
            ISessionService sessionService,
            IUserService userService,
            ILogger<ServiceController> logger)
        {
            _config = config;
            _contextAccessor = contextAccessor;
            _stubAuthenticationService = stubAuthenticationService;
            _sessionService = sessionService;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Route("signout", Name = SignoutGet)]
        public new IActionResult SignOut()
        {
            // clear session entries before sign-out
            var maybeUserId = _userService.GetUserId();
            if (maybeUserId.HasValue)
            {
                try
                {
                    _sessionService.ClearUserSession();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to clear session state for user {UserId}", maybeUserId.Value);
                }
            }

            var idToken = _contextAccessor.HttpContext.GetTokenAsync("id_token").Result;

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

        [AllowAnonymous]
        [Route("signoutcleanup")]
        public void SignOutCleanup()
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete("SFA.DAS.EmployerFeedback.Web.Auth");
        }

#if DEBUG
        [AllowAnonymous()]
        [HttpGet]
        [Route("signin-stub", Name = SignInStubGet)]
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
        [Route("signin-stub", Name = SignInStubPost)]
        public async Task<IActionResult> SigninStubPost(SignInStubViewModel model)
        {
            var claims = await _stubAuthenticationService.GetStubSignInClaims(new StubAuthUserDetails
            {
                Email = model.StubEmail,
                Id = model.StubId
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claims,
                new AuthenticationProperties());

            return RedirectToRoute(SignedInStubGet, new { model.ReturnUrl });
        }

        [Authorize()]
        [HttpGet]
        [Route("signed-in-stub", Name = SignedInStubGet)]
        public IActionResult SignedInStub(string returnUrl)
        {
            return View(new SignedInStubViewModel(_contextAccessor, returnUrl));
        }
#endif
    }
}