using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class ConfirmationControllerTests
    {
        private readonly ConfirmationController _controller;
        private readonly IFixture _fixture = new Fixture();
        private readonly SurveyModel _cachedSurveyModel;
        private ExternalLinksConfiguration _externalLinks = new ExternalLinksConfiguration
        {
            FindApprenticeshipTrainingSiteUrl = "findanapprentice.sfa.gov.uk"
        };

        public ConfirmationControllerTests()
        {
            _cachedSurveyModel = _fixture.Create<SurveyModel>();
            var sessionServiceMock = new Mock<ISessionStorageService>();
            var loggerMock = new Mock<ILogger<ConfirmationController>>();

            var config = new EmployerFeedbackWebConfiguration()
            {
                ExternalLinks = _externalLinks
            };
            var urlBuilder = new UrlBuilder("LOCAL");
            sessionServiceMock
                .Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                    .Returns(Task.FromResult(_cachedSurveyModel));
            _controller = new ConfirmationController(
                sessionServiceMock.Object,
                config,
                urlBuilder,
                loggerMock.Object);

            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(EmployerClaims.UserId, "TestUserIdValue"),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        [OneTimeTearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }


        [Test]
        public async Task ApprenticeApi_ProviderHasFeedback_FeedbackDisplayed_InViewModel()
        {
            // Arrange
            var encodedAccountId = "ABCDEFG";

            // Act
            var result = await _controller.Index(encodedAccountId) as ViewResult;

            // Assert
            var viewModel = result.Model as ConfirmationViewModel;
            viewModel.Should().NotBeNull();
            viewModel.FeedbackRating.Should().Be(_cachedSurveyModel.Rating);
            viewModel.ProviderName.Should().Be(_cachedSurveyModel.ProviderName);
            viewModel.FatUrl.ToLowerInvariant().Should().Be(_externalLinks.FindApprenticeshipTrainingSiteUrl.ToLowerInvariant());
            viewModel.EmployerAccountsHomeUrl.Should().Be($"https://accounts.local-eas.apprenticeships.education.gov.uk/accounts/{encodedAccountId}/teams");
        }

    }
}
