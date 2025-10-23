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
    [Route("")]
    public class HomeController : Controller
    {
        #region Routes;
        public const string SignoutGet = nameof(SignoutGet);
        #endregion

        private readonly IConfiguration _config;
        private readonly IStubAuthenticationService _stubAuthenticationService;
        private readonly IHttpContextAccessor _contextAccessor;

        public HomeController(
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

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.Parameters.Clear();
            authenticationProperties.Parameters.Add("id_token", idToken);

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
    }
}