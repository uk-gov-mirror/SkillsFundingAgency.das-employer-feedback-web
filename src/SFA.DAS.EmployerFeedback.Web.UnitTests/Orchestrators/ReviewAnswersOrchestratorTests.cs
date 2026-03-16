using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerFeedback;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class ReviewAnswersOrchestratorTests
    {
        private Mock<ISessionService> _mockSessionService;
        private Mock<ITrainingProviderService> _mockTrainingProviderService;
        private Mock<IAccountsLinkService> _mockAccountsLinkService;
        private Mock<IMediator> _mockMediator;
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _mockLogger;
        private EmployerFeedbackWebConfiguration _config;

        private ReviewAnswersOrchestrator _sut;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _mockSessionService = new Mock<ISessionService>();
            _mockTrainingProviderService = new Mock<ITrainingProviderService>();
            _mockAccountsLinkService = new Mock<IAccountsLinkService>();
            _mockMediator = new Mock<IMediator>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _userId = Guid.NewGuid();

            _mockUserService.Setup(x => x.GetUserId()).Returns(_userId);

            _config = new EmployerFeedbackWebConfiguration
            {
                ExternalLinks = new ExternalLinksConfiguration
                {
                    FindApprenticeshipTrainingSiteUrl = "https://fat.url",
                    ComplaintSiteUrl = "https://complaints.url",
                    ComplaintToProviderSiteUrl = "https://complaints.provider.url"
                }
            };

            _sut = new ReviewAnswersOrchestrator(
                _mockUserService.Object,
                _mockLogger.Object,
                _mockSessionService.Object,
                _mockTrainingProviderService.Object,
                _mockAccountsLinkService.Object,
                _mockMediator.Object,
                _config);
        }

        [Test]
        public async Task GetReviewAnswersViewModel_Should_Return_ViewModel_With_Survey_And_Urls()
        {
            // Arrange
            var survey = new SurveyModel 
            { 
                ProviderName = "Test provider",
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Friendly", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Timely", Good = false, Bad = true },
                    new ProviderAttributeModel { Name = "Accurate", Good = false, Bad = true}
                },
                Rating = ProviderRating.Good 
            };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);

            // Act
            var result = await _sut.GetReviewAnswersViewModel();

            // Assert
            result.Should().NotBeNull();
            result.Survey.Should().BeSameAs(survey);
            result.FatSiteUrl.Should().Be("https://fat.url");
        }

        [Test]
        public async Task CanSubmitFeedback_Should_Return_True_When_Service_Allows()
        {
            // Arrange
            var survey = new SurveyModel { ProviderName = "Another provider" };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);
            _mockTrainingProviderService.Setup(s => s.CanSubmitFeedback(survey, _userId)).ReturnsAsync(true);

            // Act
            var result = await _sut.CanSubmitFeedback();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task CanSubmitFeedback_Should_Return_False_When_Service_Denies()
        {
            // Arrange
            var survey = new SurveyModel();
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);
            _mockTrainingProviderService.Setup(s => s.CanSubmitFeedback(survey, _userId)).ReturnsAsync(false);

            // Act
            var result = await _sut.CanSubmitFeedback();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task SubmitEmployerFeedback_Should_Send_Command_With_Correct_Attributes_When_Successful()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            var survey = new SurveyModel
            {
                Ukprn = 12345678,
                AccountId = 123,
                Rating = ProviderRating.VeryPoor,
                FeedbackSource = FeedbackSource.AdHoc,
                UserRef = _userId,
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { AttributeId = 1, Good = true, Bad = false},
                    new ProviderAttributeModel { AttributeId = 2, Good = false, Bad = true }
                }
            };

            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);
            _mockMediator.Setup(m => m.Send(It.IsAny<SubmitEmployerFeedbackCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.SubmitEmployerFeedback(modelState);

            // Assert
            result.Should().BeTrue();
            _mockMediator.Verify(m => m.Send(
                It.Is<SubmitEmployerFeedbackCommand>(c =>
                    c.Ukprn == 12345678 &&
                    c.AccountId == 123 &&
                    c.UserRef == _userId &&
                    c.FeedbackSource == FeedbackSource.AdHoc &&
                    c.Attributes.Count == 2 &&
                    c.Rating == survey.Rating.ToString()),
                It.IsAny<CancellationToken>()), Times.Once);
            modelState.Should().BeEmpty();
        }

        [Test]
        public async Task SubmitEmployerFeedback_Should_Add_ModelError_When_Unsuccessful()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            var survey = new SurveyModel
            {
                Ukprn = 1234,
                AccountId = 567,
                Rating = ProviderRating.Good,
                FeedbackSource = FeedbackSource.AdHoc,
                Attributes = new List<ProviderAttributeModel>()
            };

            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);
            _mockMediator.Setup(m => m.Send(It.IsAny<SubmitEmployerFeedbackCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.SubmitEmployerFeedback(modelState);

            // Assert
            result.Should().BeFalse();
            modelState.Should().ContainKey(nameof(ReviewAnswersViewModel.Survey));
        }

        [Test]
        public async Task GetFeedbackConfirmationViewModel_Should_Return_ViewModel_And_Reset_PagingState()
        {
            // Arrange
            var account = new AccountModel { EncodedAccountId = "ENC123" };
            var survey = new SurveyModel
            {
                ProviderName = "Another provider",
                Rating = ProviderRating.Excellent
            };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);
            _mockAccountsLinkService.Setup(s => s.AccountsHome("ENC123")).Returns("https://home.url");

            // Act
            var result = await _sut.GetFeedbackConfirmationViewModel(account);

            // Assert
            result.ProviderName.Should().Be("Another provider");
            result.FeedbackRating.Should().Be(ProviderRating.Excellent);
            result.FatUrl.Should().Be("https://fat.url");
            result.ComplaintSiteUrl.Should().Be("https://complaints.url");
            result.ComplaintToProviderSiteUrl.Should().Be("https://complaints.provider.url");
            result.EmployerAccountsHomeUrl.Should().Be("https://home.url");

            _mockSessionService.Verify(s => s.SetPagingState(null), Times.Once);
        }

        [Test]
        public void GetFeedbackAlreadySubmittedViewModel_Should_Return_HomeUrl()
        {
            // Arrange
            var account = new AccountModel { EncodedAccountId = "ENC999" };
            _mockAccountsLinkService.Setup(s => s.AccountsHome("ENC999")).Returns("https://acc.url");

            // Act
            var result = _sut.GetFeedbackAlreadySubmittedViewModel(account);

            // Assert
            result.EmployerAccountsHomeUrl.Should().Be("https://acc.url");
        }
    }
}
