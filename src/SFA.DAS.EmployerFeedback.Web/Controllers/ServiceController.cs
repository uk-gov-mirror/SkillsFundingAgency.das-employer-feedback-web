using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

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

        public ServiceController(
            IConfiguration config,
            IStubAuthenticationService stubAuthenticationService,
            IHttpContextAccessor contextAccessor)
        {
            _config = config;
            _contextAccessor = contextAccessor;
            _stubAuthenticationService = stubAuthenticationService;
        }

        [HttpGet]
        [Route("signout", Name = SignoutGet)]
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