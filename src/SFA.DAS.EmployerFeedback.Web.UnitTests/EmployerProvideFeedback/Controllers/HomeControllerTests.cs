using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.GovUK.Auth.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private HomeController _controller;
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IStubAuthenticationService> _stubAuthenticationServiceMock;

        [SetUp]
        public void SetUp()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _stubAuthenticationServiceMock = new Mock<IStubAuthenticationService>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object, _configurationMock.Object, _stubAuthenticationServiceMock.Object, _httpContextAccessorMock.Object);
        }

        [Test]
        public async Task SignOut_ShouldSignOutUser()
        {
            // Arrange
            var idToken = "test_id_token";
            var httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);
            _configurationMock.Setup(c => c["StubAuth"]).Returns("false");

            var claims = new List<Claim>
            {
                new Claim("id_token", idToken)
            };

            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var authResult = AuthenticateResult.Success(new AuthenticationTicket(
                new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")),
                new AuthenticationProperties(),
                CookieAuthenticationDefaults.AuthenticationScheme));

            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(s => s.AuthenticateAsync(httpContext, CookieAuthenticationDefaults.AuthenticationScheme))
                .ReturnsAsync(authResult);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            httpContext.RequestServices = serviceProviderMock.Object;
            _httpContextAccessorMock.Setup(c => c.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = await _controller.SignOut() as SignOutResult;

            // Assert
            result.Should().NotBeNull();
            result.AuthenticationSchemes.Should().Contain(new[]
            {
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme
            });
        }

        [Test]
        public void SignOutCleanup_ShouldDeleteAuthCookie()
        {
            // Arrange
            var httpContext = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var responseCookiesMock = new Mock<IResponseCookies>();

            httpContext.Setup(c => c.Response).Returns(responseMock.Object);
            responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);

            _httpContextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext.Object);

            // Act
            _controller.SignOutCleanup();

            // Assert
            responseCookiesMock.Verify(c => c.Delete("SFA.DAS.EmployerFeedback.Web.Auth"), Times.Once);
        }

        [Test]
        public void Ping()
        {
            // Act
            var result = _controller.Ping();

            // Assert
            result.Should().BeOfType<OkResult>();

        }

        [TearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }
    }
}
