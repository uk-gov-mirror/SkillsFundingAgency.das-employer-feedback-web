using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Home;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private Mock<IConfiguration> _mockConfig;
        private Mock<IStubAuthenticationService> _mockStubAuthService;
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<IResponseCookies> _mockCookies;
        private Mock<HttpResponse> _mockResponse;
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockStubAuthService = new Mock<IStubAuthenticationService>();
            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockResponse = new Mock<HttpResponse>();
            _mockCookies = new Mock<IResponseCookies>();

            _mockResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
            _mockHttpContext.Setup(h => h.Response).Returns(_mockResponse.Object);
            _mockContextAccessor.Setup(c => c.HttpContext).Returns(_mockHttpContext.Object);

            _sut = new HomeController(_mockConfig.Object, _mockStubAuthService.Object, _mockContextAccessor.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task SignOut_Should_Return_SignOutResult_With_Correct_Schemes_When_StubAuth_False()
        {
            // Arrange
            var expectedToken = "id123";

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.AuthenticateAsync(_mockHttpContext.Object, It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(
                    ticket: new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties
                    {
                        Items = { { ".Token.id_token", expectedToken } }
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme)));

            _mockHttpContext.Setup(h => h.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            _mockConfig.Setup(c => c["StubAuth"]).Returns("false");

            // Act
            var result = await _sut.SignOut();

            // Assert
            var signOutResult = result.Should().BeOfType<SignOutResult>().Subject;
            signOutResult.AuthenticationSchemes.Should().Contain(CookieAuthenticationDefaults.AuthenticationScheme);
            signOutResult.AuthenticationSchemes.Should().Contain(OpenIdConnectDefaults.AuthenticationScheme);
            signOutResult.Properties.Parameters["id_token"].Should().Be(expectedToken);
        }

        [Test]
        public async Task SignOut_Should_Include_Only_Cookie_Scheme_When_StubAuth_True()
        {
            // Arrange
            var expectedToken = "id123";

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(a => a.AuthenticateAsync(_mockHttpContext.Object, It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(
                    ticket: new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties
                    {
                        Items = { { ".Token.id_token", expectedToken } }
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme)));

            _mockHttpContext.Setup(h => h.RequestServices.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            _mockConfig.Setup(c => c["StubAuth"]).Returns("true");

            // Act
            var result = await _sut.SignOut();

            // Assert
            var signOutResult = result.Should().BeOfType<SignOutResult>().Subject;
            signOutResult.AuthenticationSchemes.Should()
                .ContainSingle().Which.Should().Be(CookieAuthenticationDefaults.AuthenticationScheme);
            signOutResult.Properties.Parameters["id_token"].Should().Be(expectedToken);
        }


        [Test]
        public void SignOutCleanup_Should_Delete_Auth_Cookie()
        {
            // Act
            _sut.SignOutCleanup();

            // Assert
            _mockCookies.Verify(c => c.Delete("SFA.DAS.EmployerFeedback.Web.Auth"), Times.Once);
        }

        [Test]
        public void Ping_Should_Return_OkResult()
        {
            // Act
            var result = _sut.Ping();

            // Assert
            result.Should().BeOfType<OkResult>();
        }

#if DEBUG
        [Test]
        public void SigninStub_Should_Return_View_With_Configured_Model()
        {
            // Arrange
            _mockConfig.Setup(c => c["StubId"]).Returns("stub123");
            _mockConfig.Setup(c => c["StubEmail"]).Returns("stub@example.com");
            var returnUrl = "/home";

            // Act
            var result = _sut.SigninStub(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<SignInStubViewModel>().Subject;
            model.StubId.Should().Be("stub123");
            model.StubEmail.Should().Be("stub@example.com");
            model.ReturnUrl.Should().Be(returnUrl);
        }

        [Test]
        public async Task SigninStubPost_Should_SignIn_And_Redirect_To_SignedInStub()
        {
            // Arrange
            var model = new SignInStubViewModel
            {
                StubId = "abc",
                StubEmail = "test@unit.com",
                ReturnUrl = "/return"
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("email", "test@unit.com") }));

            _mockStubAuthService.Setup(s => s.GetStubSignInClaims(It.Is<StubAuthUserDetails>(
                    d => d.Email == "test@unit.com" && d.Id == "abc")))
                .ReturnsAsync(claimsPrincipal);

            var authServiceMock = new Mock<IAuthenticationService>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            _mockHttpContext.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);
            _sut.ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object };

            // Act
            var result = await _sut.SigninStubPost(model);

            // Assert
            authServiceMock.Verify(a => a.SignInAsync(
                _mockHttpContext.Object,
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                It.IsAny<AuthenticationProperties>()), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(HomeController.SignedInStubGet);
            redirect.RouteValues["ReturnUrl"].Should().Be("/return");
        }


        [Test]
        public void SignedInStub_Should_Return_View_With_Model()
        {
            // Arrange
            var returnUrl = "/dashboard";

            // Act
            var result = _sut.SignedInStub(returnUrl);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<SignedInStubViewModel>();
        }
#endif

        [Test]
        public void SignOut_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(HomeController)
                .GetMethods()
                .Single(m => m.Name == nameof(HomeController.SignOut) && m.DeclaringType == typeof(HomeController));

            // Act
            var routeAttr = method.GetCustomAttributes(typeof(RouteAttribute), false)
                .Cast<RouteAttribute>().Single();

            // Assert
            routeAttr.Name.Should().Be(HomeController.SignoutGet);
        }


#if DEBUG
        [Test]
        public void SigninStub_Should_Have_Correct_Route_Name()
        {
            var method = typeof(HomeController).GetMethod(nameof(HomeController.SigninStub));
            var routeAttr = method.GetCustomAttributes(typeof(RouteAttribute), false)
                .Cast<RouteAttribute>().Single();

            routeAttr.Name.Should().Be(HomeController.SignInStubGet);
        }

        [Test]
        public void SigninStubPost_Should_Have_Correct_Route_Name()
        {
            var method = typeof(HomeController).GetMethod(nameof(HomeController.SigninStubPost));
            var routeAttr = method.GetCustomAttributes(typeof(RouteAttribute), false)
                .Cast<RouteAttribute>().Single();

            routeAttr.Name.Should().Be(HomeController.SignInStubPost);
        }

        [Test]
        public void SignedInStub_Should_Have_Correct_Route_Name()
        {
            var method = typeof(HomeController).GetMethod(nameof(HomeController.SignedInStub));
            var routeAttr = method.GetCustomAttributes(typeof(RouteAttribute), false)
                .Cast<RouteAttribute>().Single();

            routeAttr.Name.Should().Be(HomeController.SignedInStubGet);
        }
#endif
    }
}
