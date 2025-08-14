using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Domain.Interfaces;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.TestHelper.Extensions;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models;
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
        private Mock<IConfiguration> _configMock;
        private Mock<UrlBuilder> _urlBuilderMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private Mock<IStubAuthenticationService> _stubAuthServiceMock;
        private Mock<IEmployerFeedbackOuterApi> _outerApiMock;
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _configMock = new Mock<IConfiguration>();
            _urlBuilderMock = new Mock<UrlBuilder>("LOCAL");
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _stubAuthServiceMock = new Mock<IStubAuthenticationService>();
            _outerApiMock = new Mock<IEmployerFeedbackOuterApi>();

            _sut = new HomeController(
                _configMock.Object,
                _urlBuilderMock.Object,
                _contextAccessorMock.Object,
                _loggerMock.Object,
                _stubAuthServiceMock.Object,
                _outerApiMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningLocally()
        {
            // Arrange
            _configMock
                .Setup(p => p["EnvironmentName"])
                .Returns("LOCAL");

            // Act
            var result = _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningInDev()
        {
            // Arrange
            _configMock
                .Setup(p => p["EnvironmentName"])
                .Returns("DEV");

            // Act
            var result = _sut.Index() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void AccessDenied_ShouldReturnView()
        {
            // Act
            var result = _sut.AccessDenied() as ViewResult;

            // Assert
            result.Should().NotBeNull();
        }

        [Test]
        public void Error_ShouldLogErrorAndReturnView()
        {
            // Arrange
            var errorMessage = "Test error message";
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "TestTraceIdentifier";
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);

            // Act
            var result = _sut.Error(errorMessage) as ViewResult;

            // Assert
            _loggerMock.VerifyLogError(errorMessage, Times.Once);

            result.Should().NotBeNull();
            var model = result.Model as ErrorViewModel;
            model.Should().NotBeNull();
            model.RequestId.Should().Be("TestTraceIdentifier");
            model.ErrorMessage.Should().Be(errorMessage);
        }

        [Test]
        public async Task SignOut_ShouldSignOutUser()
        {
            // Arrange
            var idToken = "test_id_token";
            var httpContext = new DefaultHttpContext();
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext);
            _configMock.Setup(c => c["StubAuth"]).Returns("false");

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

            _contextAccessorMock.Setup(c => c.HttpContext.RequestServices).Returns(serviceProviderMock.Object);

            // Act
            var result = await _sut.SignOut() as SignOutResult;

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

            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContext.Object);

            // Act
            _sut.SignOutCleanup();

            // Assert
            responseCookiesMock.Verify(c => c.Delete("SFA.DAS.EmployerFeedback.Web.Auth"), Times.Once);
        }
    }
}