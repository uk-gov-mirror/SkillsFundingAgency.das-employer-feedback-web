using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Application.Queries.GetAllQuestionAttributes;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Exceptions;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class ProviderOrchestratorTests
    {
        private Mock<ISessionService> _mockSessionService;
        private Mock<ITrainingProviderService> _mockTrainingProviderService;
        private Mock<IMediator> _mockMediator;
        private Mock<IAccountsLinkService> _mockAccountsLinkService;
        private Mock<IValidator<ProviderConfirmViewModel>> _mockValidator;
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<ProviderOrchestrator>> _mockLogger;

        private ProviderOrchestrator _sut;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _mockSessionService = new Mock<ISessionService>();
            _mockTrainingProviderService = new Mock<ITrainingProviderService>();
            _mockMediator = new Mock<IMediator>();
            _mockAccountsLinkService = new Mock<IAccountsLinkService>();
            _mockValidator = new Mock<IValidator<ProviderConfirmViewModel>>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<ProviderOrchestrator>>();
            _userId = Guid.NewGuid();

            _mockUserService.Setup(x => x.GetUserId()).Returns(_userId);

            _sut = new ProviderOrchestrator(
                _mockSessionService.Object,
                _mockUserService.Object,
                _mockLogger.Object,
                _mockTrainingProviderService.Object,
                _mockMediator.Object,
                _mockAccountsLinkService.Object,
                _mockValidator.Object);
        }

        [Test]
        public async Task GetProviderSearchViewModel_Should_Return_ViewModel_With_Routes_And_BackUrl()
        {
            // Arrange
            var model = new ProviderSearchRequestModel { AccountId = 1, EncodedAccountId = "ABC123" };
            var pagingState = new PagingState
            {
                PageIndex = 2,
                PageSize = 10,
                SelectedFeedbackStatus = "Submitted",
                SelectedProviderName = "Test",
                SortColumn = SortColumn.Default,
                SortOrder = SortOrder.Ascending
            };
            var viewModel = new ProviderSearchViewModel();
            _mockSessionService.Setup(s => s.GetPagingState(_userId)).ReturnsAsync(pagingState);
            _mockTrainingProviderService.Setup(t => t.GetTrainingProviderSearchViewModel(
                model.AccountId, model.EncodedAccountId, _userId, "Test", "Submitted", 10, 2, SortColumn.Default, SortOrder.Ascending))
                .ReturnsAsync(viewModel);
            _mockAccountsLinkService.Setup(a => a.AccountsHome("ABC123")).Returns("https://back");

            // Act
            var result = await _sut.GetProviderSearchViewModel(model);

            // Assert
            result.Should().BeSameAs(viewModel);
            result.ChangePageRouteName.Should().Be(ProviderController.ProviderSearchGet);
            result.BackUrl.Should().Be("https://back");
        }

        [Test]
        public async Task SetProviderSearchPageIndex_Should_Update_PageIndex()
        {
            // Arrange
            var newPageIndex = 5;

            var pagingState = new PagingState
            {
                PageIndex = PagingState.DefaultPageIndex
            };

            Action<PagingState> capturedAction = null;
            _mockSessionService
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .Callback<Guid, Action<PagingState>>((_, action) => capturedAction = action)
                .ReturnsAsync(new PagingState()); // no need to check what is returned

            // Act
            await _sut.SetProviderSearchPageIndex(newPageIndex);

            // Assert
            capturedAction.Should().NotBeNull("the orchestrator should call UpdatePagingState with an action");

            // apply the captured lambda to fake PagingState to inspect what happens
            capturedAction(pagingState);

            pagingState.PageIndex.Should().Be(5);
        }

        [Test]
        public async Task SetProviders_Should_Save_Providers_To_Session()
        {
            // Arrange
            var viewModel = new ProviderSearchViewModel
            {
                Providers = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(
                    new List<ProviderSearchViewModel.EmployerTrainingProvider>
                    {
                        new ProviderSearchViewModel.EmployerTrainingProvider { ProviderId = 123, ProviderName = "Test" }
                    }, 1, 1, 10, 10)
            };

            // Act
            await _sut.SetProviders(viewModel.Providers.Items);

            // Assert
            _mockSessionService.Verify(s =>
                s.SetProviders(_userId, viewModel.Providers.Items), Times.Once);
        }

        [TestCase(FeedbackSource.AdHoc)]
        [TestCase(FeedbackSource.Email)]
        public async Task SetFeedbackSource_Should_Save_To_Session(FeedbackSource feedbackSource)
        {
            // Act
            await _sut.SetFeedbackSource(feedbackSource);

            // Assert
            _mockSessionService.Verify(s => s.SetFeedbackSource(_userId, feedbackSource), Times.Once);
        }

        [Test]
        public async Task UpdateProviderSearchFilters_Should_Reset_PageIndex_And_Update_Filters()
        {
            // Arrange
            var viewModel = new ProviderSearchViewModel
            {
                SelectedFeedbackStatus = "Submitted",
                SelectedProviderName = "Name"
            };

            var pagingState = new PagingState
            {
                PageIndex = 5,
                SelectedFeedbackStatus = "OldStatus",
                SelectedProviderName = "OldName"
            };

            Action<PagingState> capturedAction = null;
            _mockSessionService
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .Callback<Guid, Action<PagingState>>((_, action) => capturedAction = action)
                .ReturnsAsync(new PagingState()); // no need to check what is returned

            // Act
            await _sut.UpdateProviderSearchFilters(viewModel);

            // Assert
            capturedAction.Should().NotBeNull("the orchestrator should call UpdatePagingState with an action");

            // apply the captured lambda to fake PagingState to inspect what happens
            capturedAction(pagingState);

            pagingState.PageIndex.Should().Be(PagingState.DefaultPageIndex);
            pagingState.SelectedFeedbackStatus.Should().Be("Submitted");
            pagingState.SelectedProviderName.Should().Be("Name");
        }

        [Test]
        public async Task ClearProviderSearchFilters_Should_Set_New_PagingState()
        {
            // Act
            await _sut.ClearProviderSearchFilters();

            // Assert
            _mockSessionService.Verify(s => s.SetPagingState(
                _userId,
                It.Is<PagingState>(p =>
                    p.PageIndex == PagingState.DefaultPageIndex &&
                    p.PageSize == PagingState.DefaultPageSize &&
                    p.SortColumn == SortColumn.Default &&
                    p.SortOrder == SortOrder.Ascending &&
                    string.IsNullOrEmpty(p.SelectedProviderName) &&
                    string.IsNullOrEmpty(p.SelectedFeedbackStatus)
                )),
                Times.Once);

            _mockSessionService.Verify(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()), Times.Never);
        }

        [Test]
        public async Task SortProviderSearch_Should_Update_SortColumn_And_Order()
        {
            // Arrange
            var model = new ProviderSearchSortRequestModel
            {
                SortColumn = SortColumn.FeedbackStatus,
                SortOrder = SortOrder.Descending
            };

            var pagingState = new PagingState
            {
                SortColumn = SortColumn.Default,
                SortOrder = SortOrder.Ascending
            };

            Action<PagingState> capturedAction = null;
            _mockSessionService
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .Callback<Guid, Action<PagingState>>((_, action) => capturedAction = action)
                .ReturnsAsync(new PagingState());

            // Act
            await _sut.SortProviderSearch(model.SortColumn, model.SortOrder);

            // Assert
            capturedAction.Should().NotBeNull("the orchestrator should call UpdatePagingState with an action");

            // apply the captured lambda to fake PagingState to inspect what happens
            capturedAction(pagingState);

            pagingState.SortColumn.Should().Be(SortColumn.FeedbackStatus);
            pagingState.SortOrder.Should().Be(SortOrder.Descending);
        }

        [Test]
        public async Task GetProviderConfirmViewModel_Should_Return_ViewModel_When_Provider_Found()
        {
            // Arrange
            var providers = new List<ProviderSearchViewModel.EmployerTrainingProvider>
            {
                new ProviderSearchViewModel.EmployerTrainingProvider { ProviderId = 999, ProviderName = "Found" }
            };
            _mockSessionService.Setup(s => s.GetProviders(_userId)).ReturnsAsync(providers);
            var model = new ProviderConfirmRequestModel { ProviderId = 999, EncodedAccountId = "ABC123" };

            // Act
            var result = await _sut.GetProviderConfirmViewModel(model);

            // Assert
            result.Should().NotBeNull();
            result.ProviderName.Should().Be("Found");
            result.ProviderId.Should().Be(999);
            result.EncodedAccountId.Should().Be("ABC123");
        }

        [Test]
        public async Task GetProviderConfirmViewModel_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            _mockSessionService.Setup(s => s.GetProviders(_userId))
                .ReturnsAsync(new List<ProviderSearchViewModel.EmployerTrainingProvider>());
            var model = new ProviderConfirmRequestModel { ProviderId = 111 };

            // Act
            var result = await _sut.GetProviderConfirmViewModel(model);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ValidateProviderConfirmViewModel_Should_Return_True_When_Valid()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel();
            var modelState = new ModelStateDictionary();
            _mockValidator.Setup(v => v.ValidateAsync(viewModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.ValidateProviderConfirmViewModel(viewModel, modelState);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateProviderConfirmViewModel_Should_Return_False_When_Invalid()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel();
            var modelState = new ModelStateDictionary();
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Field", "Error message")
            };
            _mockValidator.Setup(v => v.ValidateAsync(viewModel, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _sut.ValidateProviderConfirmViewModel(viewModel, modelState);

            // Assert
            result.Should().BeFalse();
            modelState.Should().ContainKey("Field");
        }

        [Test]
        public async Task CreateNewSurvey_Should_Save_SurveyModel_To_Session()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel
            {
                AccountId = 100,
                EncodedAccountId = "ENC123",
                ProviderId = 12345678,
                ProviderName = "Test Provider"
            };

            var attributes = new List<QuestionAttribute>
            {
                new QuestionAttribute { AttributeId = 1, AttributeName = "Helpful" }
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQuestionAttributesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attributes);
            _mockSessionService.Setup(s => s.GetFeedbackSource(_userId)).ReturnsAsync(FeedbackSource.AdHoc);

            // Act
            await _sut.CreateNewSurvey(viewModel);

            // Assert
            _mockSessionService.Verify(s => s.SetSurveyModel(_userId,
                It.Is<SurveyModel>(m =>
                    m.AccountId == 100 &&
                    m.EncodedAccountId == "ENC123" &&
                    m.Ukprn == 12345678 &&
                    m.ProviderName == "Test Provider" &&
                    m.Attributes.Any(a => a.AttributeId == 1 && a.Name == "Helpful") &&
                    m.FeedbackSource == FeedbackSource.AdHoc)), Times.Once);
        }

        [Test]
        public async Task CreateNewSurvey_Should_Default_FeedbackSource_To_AdHoc_When_Not_Set_In_Session()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel
            {
                AccountId = 200,
                EncodedAccountId = "ENC999",
                ProviderId = 87654321,
                ProviderName = "Another Provider"
            };

            var attributes = new List<QuestionAttribute>
            {
                new QuestionAttribute { AttributeId = 2, AttributeName = "Friendly" }
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQuestionAttributesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(attributes);

            _mockSessionService.Setup(s => s.GetFeedbackSource(_userId)).ReturnsAsync((FeedbackSource?)null);

            // Act
            await _sut.CreateNewSurvey(viewModel);

            // Assert
            _mockSessionService.Verify(s => s.SetFeedbackSource(_userId, FeedbackSource.AdHoc), Times.Once);

            _mockSessionService.Verify(s => s.SetSurveyModel(_userId,
                It.Is<SurveyModel>(m =>
                    m.AccountId == 200 &&
                    m.EncodedAccountId == "ENC999" &&
                    m.Ukprn == 87654321 &&
                    m.ProviderName == "Another Provider" &&
                    m.Attributes.Any(a => a.AttributeId == 2 && a.Name == "Friendly") &&
                    m.FeedbackSource == FeedbackSource.AdHoc)), Times.Once);
        }

        [Test]
        public void CreateNewSurvey_Should_Throw_When_Attributes_Null()
        {
            // Arrange
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAllQuestionAttributesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<QuestionAttribute>)null);
            var viewModel = new ProviderConfirmViewModel();

            // Act
            var act = async () => await _sut.CreateNewSurvey(viewModel);

            // Assert
            act.Should().ThrowAsync<EmployerFeedbackException>()
                .WithMessage("Unable to load question attributes");
        }
    }
}
