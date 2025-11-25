using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.GovUK.Auth.Configuration;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private UrlBuilder _urlBuilder;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _sut;

        [SetUp]
        public void Setup()
        {
            _urlBuilder = new UrlBuilder("LOCAL");
            _configurationMock = new Mock<IConfiguration>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _loggerMock = new Mock<ILogger<HomeController>>();

            _sut = new HomeController(
                _urlBuilder,
                _configurationMock.Object,
                _contextAccessorMock.Object,
                _loggerMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public void Ping_ShouldReturnOkResult()
        {
            // Act
            var result = _sut.Ping();

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningLocally()
        {
            _configurationMock
                .Setup(c => c["EnvironmentName"])
                .Returns("LOCAL");

            var result = _sut.Index() as ViewResult;

            result.Should().NotBeNull();
        }

        [Test]
        public void Index_ShouldReturnView_WhenRunningInDev()
        {
            _configurationMock
                .Setup(c => c["EnvironmentName"])
                .Returns("DEV");

            var result = _sut.Index() as ViewResult;

            result.Should().NotBeNull();
        }

        [Test]
        public void Index_ShouldRedirectToAccountsLink_WhenNotRunningLocallyOrDev()
        {
            _configurationMock
                .Setup(c => c["EnvironmentName"])
                .Returns("PROD");

            var result = _sut.Index() as RedirectResult;

            result.Should().NotBeNull();
            result.Url.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void ProvideFeedbackStub_ShouldDeleteCookieAndRedirect()
        {
            var httpContextMock = new Mock<HttpContext>();
            var responseMock = new Mock<HttpResponse>();
            var responseCookiesMock = new Mock<IResponseCookies>();

            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);
            responseMock.Setup(r => r.Cookies).Returns(responseCookiesMock.Object);
            _contextAccessorMock.Setup(c => c.HttpContext).Returns(httpContextMock.Object);

            var result = _sut.ProvideFeedbackStub() as RedirectToRouteResult;

            result.Should().NotBeNull();
            result.RouteName.Should().Be(ProviderController.ProviderSearchGet);

            responseCookiesMock.Verify(c => c.Delete(GovUkConstants.StubAuthCookieName), Times.Once);

            result.RouteValues.Should().ContainKey("encodedAccountId");
            result.RouteValues["encodedAccountId"].Should().Be("{{hashedAccountId}}");
        }
    }
}
