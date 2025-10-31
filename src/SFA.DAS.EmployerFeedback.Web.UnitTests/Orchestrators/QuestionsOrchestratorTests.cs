using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class QuestionsOrchestratorTests
    {
        private Mock<ISessionStorageService> _mockSessionService;
        private Mock<IValidator<QuestionOneStrengthsViewModel>> _mockQ1Validator;
        private Mock<IValidator<QuestionTwoWeaknessesViewModel>> _mockQ2Validator;
        private Mock<IValidator<QuestionThreeRatingViewModel>> _mockQ3Validator;
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<QuestionsOrchestrator>> _mockLogger;

        private QuestionsOrchestrator _sut;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _mockSessionService = new Mock<ISessionStorageService>();
            _mockQ1Validator = new Mock<IValidator<QuestionOneStrengthsViewModel>>();
            _mockQ2Validator = new Mock<IValidator<QuestionTwoWeaknessesViewModel>>();
            _mockQ3Validator = new Mock<IValidator<QuestionThreeRatingViewModel>>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<QuestionsOrchestrator>>();
            _userId = Guid.NewGuid();

            _mockUserService.Setup(x => x.GetUserId()).Returns(_userId);

            _sut = new QuestionsOrchestrator(
                _mockSessionService.Object,
                _mockLogger.Object,
                _mockUserService.Object,
                _mockQ1Validator.Object,
                _mockQ2Validator.Object,
                _mockQ3Validator.Object);
        }

        [Test]
        public async Task GetStartFeedbackViewModel_Should_Return_ViewModel_With_EncodedAccountId_And_ProviderName()
        {
            // Arrange
            var accountModel = new AccountModel { EncodedAccountId = "ABC123" };
            var survey = new SurveyModel { ProviderName = "Test provider" };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);

            // Act
            var result = await _sut.GetStartFeedbackViewModel(accountModel);

            // Assert
            result.EncodedAccountId.Should().Be("ABC123");
            result.ProviderName.Should().Be("Test provider");
        }

        [Test]
        public async Task GetQuestionOneStrengthsViewModel_Should_Map_Survey_To_ViewModel()
        {
            // Arrange
            var model = new QuestionRequestModel { ReturnToReviewAnswers = true };
            var survey = new SurveyModel
            {
                EncodedAccountId = "ENC123",
                ProviderName = "Another test provider",
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = true, Bad = false }
                }
            };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);

            // Act
            var result = await _sut.GetQuestionOneStrengthsViewModel(model);

            // Assert
            result.EncodedAccountId.Should().Be("ENC123");
            result.ProviderName.Should().Be("Another test provider");
            result.ReturnToReviewAnswers.Should().BeTrue();
            result.Attributes.Should().ContainSingle(a => a.Name == "Helpful" && a.Good && !a.Bad);
        }

        [Test]
        public async Task ValidateQuestionOneStrengthsViewModel_Should_Return_True_When_Valid()
        {
            // Arrange
            var viewModel = new QuestionOneStrengthsViewModel();
            var modelState = new ModelStateDictionary();
            _mockQ1Validator.Setup(v => v.ValidateAsync(viewModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.ValidateQuestionOneStrengthsViewModel(viewModel, modelState);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task UpdateQuestionOneAnswers_Should_Update_Good_Values_In_Survey()
        {
            // Arrange
            var survey = new SurveyModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Friendly", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Timely", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Accurate", Good = false, Bad = false }
                }
            };
            var updatedViewModel = new QuestionOneStrengthsViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = true, Bad = false},
                    new ProviderAttributeModel { Name = "Friendly", Good = true, Bad = false },
                    new ProviderAttributeModel { Name = "Timely", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Accurate", Good = false, Bad = false }
                }
            };

            Action<SurveyModel> capturedAction = null;
            _mockSessionService.Setup(s => s.UpdateSurveyModel(_userId, It.IsAny<Action<SurveyModel>>()))
                .Callback<Guid, Action<SurveyModel>>((_, action) => capturedAction = action)
                .ReturnsAsync(new SurveyModel());

            // Act
            await _sut.UpdateQuestionOneAnswers(updatedViewModel);

            // Assert
            capturedAction.Should().NotBeNull();
            capturedAction(survey);

            survey.Attributes.Should().Contain(a => a.Name == "Helpful" && a.Good && !a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Friendly" && a.Good && !a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Timely" && !a.Good && !a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Accurate" && !a.Good && !a.Bad);
        }

        [Test]
        public async Task GetQuestionTwoWeaknessesViewModel_Should_Map_Survey_To_ViewModel()
        {
            // Arrange
            var model = new QuestionRequestModel { ReturnToReviewAnswers = false };
            var survey = new SurveyModel
            {
                EncodedAccountId = "ENC2",
                ProviderName = "Provider2",
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = false, Bad = true }
                }
            };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);

            // Act
            var result = await _sut.GetQuestionTwoWeaknessesViewModel(model);

            // Assert
            result.EncodedAccountId.Should().Be("ENC2");
            result.ProviderName.Should().Be("Provider2");
            result.ReturnToReviewAnswers.Should().BeFalse();
            result.Attributes.Should().ContainSingle(a => a.Name == "Helpful" && !a.Good && a.Bad);
        }

        [Test]
        public async Task ValidateQuestionTwoWeaknessesViewModel_Should_Return_False_When_Invalid()
        {
            // Arrange
            var viewModel = new QuestionTwoWeaknessesViewModel();
            var modelState = new ModelStateDictionary();
            var failures = new List<ValidationFailure> { new ValidationFailure("Attr", "Error") };
            _mockQ2Validator.Setup(v => v.ValidateAsync(viewModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _sut.ValidateQuestionTwoWeaknessesViewModel(viewModel, modelState);

            // Assert
            result.Should().BeFalse();
            modelState.Should().ContainKey("Attr");
        }

        [Test]
        public async Task UpdateQuestionTwoAnswers_Should_Update_Bad_Values_In_Survey()
        {
            // Arrange
            var survey = new SurveyModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Friendly", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Timely", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Accurate", Good = false, Bad = false}
                }
            };
            var updatedViewModel = new QuestionTwoWeaknessesViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Name = "Helpful", Good = false, Bad = true },
                    new ProviderAttributeModel { Name = "Friendly", Good = false, Bad = true },
                    new ProviderAttributeModel { Name = "Timely", Good = false, Bad = false },
                    new ProviderAttributeModel { Name = "Accurate", Good = false, Bad = false}
                }
            };

            Action<SurveyModel> capturedAction = null;
            _mockSessionService.Setup(s => s.UpdateSurveyModel(_userId, It.IsAny<Action<SurveyModel>>()))
                .Callback<Guid, Action<SurveyModel>>((_, action) => capturedAction = action)
                .ReturnsAsync(new SurveyModel());

            // Act
            await _sut.UpdateQuestionTwoAnswers(updatedViewModel);

            // Assert
            capturedAction.Should().NotBeNull();
            capturedAction(survey);

            survey.Attributes.Should().Contain(a => a.Name == "Helpful" && !a.Good && a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Friendly" && !a.Good && a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Timely" && !a.Good && !a.Bad);
            survey.Attributes.Should().Contain(a => a.Name == "Accurate" && !a.Good && !a.Bad);
        }

        [Test]
        public async Task GetQuestionThreeRatingViewModel_Should_Map_Survey_To_ViewModel()
        {
            // Arrange
            var model = new QuestionRequestModel { ReturnToReviewAnswers = true };
            var survey = new SurveyModel
            {
                EncodedAccountId = "ENC321",
                ProviderName = "Provider new",
                Rating = ProviderRating.Excellent
            };
            _mockSessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(survey);

            // Act
            var result = await _sut.GetQuestionThreeRatingViewModel(model);

            // Assert
            result.EncodedAccountId.Should().Be("ENC321");
            result.ProviderName.Should().Be("Provider new");
            result.Rating.Should().Be(ProviderRating.Excellent);
            result.ReturnToReviewAnswers.Should().BeTrue();
        }

        [Test]
        public async Task ValidateQuestionThreeRatingViewModel_Should_Return_True_When_Valid()
        {
            // Arrange
            var viewModel = new QuestionThreeRatingViewModel();
            var modelState = new ModelStateDictionary();
            _mockQ3Validator.Setup(v => v.ValidateAsync(viewModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.ValidateQuestionThreeRatingViewModel(viewModel, modelState);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task UpdateQuestionThreeAnswers_Should_Update_Rating_In_Survey()
        {
            // Arrange
            var survey = new SurveyModel { Rating = ProviderRating.Poor };
            var updatedViewModel = new QuestionThreeRatingViewModel { Rating = ProviderRating.Good };

            Action<SurveyModel> capturedAction = null;
            _mockSessionService.Setup(s => s.UpdateSurveyModel(_userId, It.IsAny<Action<SurveyModel>>()))
                .Callback<Guid, Action<SurveyModel>>((_, action) => capturedAction = action)
                .ReturnsAsync(new SurveyModel());

            // Act
            await _sut.UpdateQuestionThreeAnswers(updatedViewModel);

            // Assert
            capturedAction.Should().NotBeNull();
            capturedAction(survey);

            survey.Rating.Should().Be(ProviderRating.Good);
        }
    }
}
