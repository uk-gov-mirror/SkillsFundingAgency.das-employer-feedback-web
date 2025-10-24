using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using static SFA.DAS.EmployerFeedback.Web.Models.Shared.ProviderSearchViewModel;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private Mock<ISessionStorageService> _session;
        private Mock<ITrainingProviderService> _providers;
        private Mock<ILogger<ProviderController>> _logger;
        private Mock<IEmployerFeedbackOuterApi> _outerApi;
        private Mock<IAccountsLinkService> _accountsLinkService;
        private Mock<IUserService> _userService;

        private ProviderController _sut;
        private Guid _userId;

        [SetUp]
        public void SetUp()
        {
            _session = new Mock<ISessionStorageService>();
            _providers = new Mock<ITrainingProviderService>();
            _logger = new Mock<ILogger<ProviderController>>();
            _outerApi = new Mock<IEmployerFeedbackOuterApi>();
            _accountsLinkService = new Mock<IAccountsLinkService>();
            _userService = new Mock<IUserService>();

            _userId = Guid.NewGuid();
            _userService.Setup(u => u.GetUserId()).Returns(_userId);

            _sut = new ProviderController(
                _session.Object,
                _providers.Object,
                _logger.Object,
                _outerApi.Object,
                _accountsLinkService.Object,
                _userService.Object);
        }

        [TearDown]
        public void TearDown() => _sut?.Dispose();

        [Test]
        public async Task ProviderSearch_GET_ShouldReturnView_WithViewModel_AndPersistSessionBits()
        {
            // Arrange
            var requestModel = new ProviderSearchRequestModel
            {
                AccountId = 12345,
                EncodedAccountId = "ABC123",
                FeedbackSource = FeedbackSource.AdHoc
            };

            var pagingStateReturned = new PagingState
            {
                PageIndex = 1,
                PageSize = 10,
                SelectedFeedbackStatus = "All",
                SelectedProviderName = "Acme",
                SortColumn = "Name",
                SortOrder = "asc"
            };

            _session
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .ReturnsAsync(pagingStateReturned);

            var list = new List<EmployerTrainingProvider>
            {
                new EmployerTrainingProvider { ProviderId = 12345678, ProviderName = "Acme Training" }
            };

            var searchVm = new ProviderSearchViewModel
            {
                AccountId = requestModel.AccountId,
                EncodedAccountId = requestModel.EncodedAccountId,
                Providers = new PaginatedList<EmployerTrainingProvider>(list, list.Count, 1, 10, 6)
            };

            _providers
                .Setup(p => p.GetTrainingProviderSearchViewModel(
                    requestModel.AccountId,
                    requestModel.EncodedAccountId,
                    _userId,
                    pagingStateReturned.SelectedProviderName,
                    pagingStateReturned.SelectedFeedbackStatus,
                    pagingStateReturned.PageSize,
                    pagingStateReturned.PageIndex,
                    pagingStateReturned.SortColumn,
                    pagingStateReturned.SortOrder))
                .ReturnsAsync(searchVm);

            _accountsLinkService
                .Setup(u => u.AccountsHome(requestModel.EncodedAccountId))
                .Returns("https://example/accounts/A12B34/home");

            // Act
            var result = await _sut.ProviderSearch(requestModel) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var vm = result.Model as ProviderSearchViewModel;
            vm.Should().NotBeNull();
            vm.Providers.Items.Should().HaveCount(1);
            vm.BackUrl.Should().Be("https://example/accounts/A12B34/home");
            vm.ChangePageAction.Should().Be(nameof(ProviderController.ProviderSearch));

            _session.Verify(s => s.SetProviders(_userId, list), Times.Once);
            _session.Verify(s => s.SetFeedbackSource(_userId, requestModel.FeedbackSource), Times.Once);
        }

        [Test]
        public async Task ProviderSearch_GET_OnException_ShouldRedirectToErrorRoute()
        {
            // Arrange
            var req = new ProviderSearchRequestModel { AccountId = 1, EncodedAccountId = "X" };
            _session
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _sut.ProviderSearch(req) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task ProviderSearch_POST_ShouldUpdatePagingState_AndRedirectToGet()
        {
            // Arrange
            var vm = new ProviderSearchViewModel
            {
                EncodedAccountId = "ABC123",
                SelectedProviderName = "Acme",
                SelectedFeedbackStatus = "All"
            };

            _session
                .Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                .ReturnsAsync(new PagingState());

            // Act
            var result = await _sut.ProviderSearch(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            result.RouteValues!["EncodedAccountId"].Should().Be(vm.EncodedAccountId);

            _session.Verify(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()), Times.Once);
        }

        [Test]
        public async Task ProviderSearch_POST_OnException_ShouldRedirectToErrorRoute()
        {
            // Arrange
            var vm = new ProviderSearchViewModel { EncodedAccountId = "Y" };
            _session.Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                    .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _sut.ProviderSearch(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task SortProviders_ShouldUpdateSort_AndRedirectToGet()
        {
            // Arrange
            var acc = new AccountModel { EncodedAccountId = "ABC123", AccountId = 12345 };
            _session.Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                    .ReturnsAsync(new PagingState());

            // Act
            var result = await _sut.SortProviders(acc, "Name", "desc") as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            result.RouteValues!["EncodedAccountId"].Should().Be(acc.EncodedAccountId);
            _session.Verify(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()), Times.Once);
        }

        [Test]
        public async Task SortProviders_OnException_ShouldRedirectToErrorRoute()
        {
            // Arrange
            var acc = new AccountModel { EncodedAccountId = "ABC123" };
            _session.Setup(s => s.UpdatePagingState(_userId, It.IsAny<Action<PagingState>>()))
                    .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _sut.SortProviders(acc, "Name", "asc") as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task ClearFilters_ShouldResetPagingState_AndRedirectToGet()
        {
            // Arrange
            var acc = new AccountModel { EncodedAccountId = "ABC123" };

            // Act
            var result = await _sut.ClearFilters(acc) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            result.RouteValues["EncodedAccountId"].Should().Be(acc.EncodedAccountId);

            _session.Verify(s => s.SetPagingState(_userId, It.Is<PagingState>(p => p.PageIndex == PagingState.DefaultPageIndex)), Times.Once);
        }

        [Test]
        public async Task ClearFilters_OnException_ShouldRedirectToErrorRoute()
        {
            // Arrange
            var acc = new AccountModel { EncodedAccountId = "ABC123" };
            _session
                .Setup(s => s.SetPagingState(_userId, It.IsAny<PagingState>()))
                .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _sut.ClearFilters(acc) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task ProviderConfirm_GET_ShouldBuildViewModel_FromSessionProviders()
        {
            // Arrange
            var req = new ProviderConfirmRequestModel
            {
                EncodedAccountId = "ABC123",
                ProviderId = 12345678
            };

            _session.Setup(s => s.GetProviders(_userId))
                .ReturnsAsync(new List<EmployerTrainingProvider>
                {
                    new EmployerTrainingProvider { ProviderId = 12345678, ProviderName = "Acme Training" }
                });

            // Act
            var result = await _sut.ProviderConfirm(req) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            var vm = result!.Model as ProviderConfirmViewModel;
            vm.Should().NotBeNull();
            vm.EncodedAccountId.Should().Be(req.EncodedAccountId);
            vm.ProviderId.Should().Be(req.ProviderId);
            vm.ProviderName.Should().Be("Acme Training");
        }

        [Test]
        public async Task ProviderConfirm_GET_OnException_ShouldRedirectToErrorRoute()
        {
            // Arrange
            var req = new ProviderConfirmRequestModel { EncodedAccountId = "X", ProviderId = 1 };
            _session.Setup(s => s.GetProviders(_userId))
                    .ThrowsAsync(new Exception("boom"));

            // Act
            var result = await _sut.ProviderConfirm(req) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task ProviderConfirm_POST_WhenNoSelection_ShouldRedirectBackToConfirm()
        {
            // Arrange
            var vm = new ProviderConfirmViewModel
            {
                EncodedAccountId = "ABC123",
                ProviderId = 12345678,
                Confirmed = null
            };

            // Act
            var result = await _sut.ProviderConfirm(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ProviderController.ProviderConfirmGet);
            result.RouteValues!["encodedAccountId"].Should().Be(vm.EncodedAccountId);
            result.RouteValues!["providerId"].Should().Be(vm.ProviderId);
        }

        [Test]
        public async Task ProviderConfirm_POST_WhenNoSelected_ShouldRedirectToSearch()
        {
            // Arrange
            var vm = new ProviderConfirmViewModel
            {
                EncodedAccountId = "ABC123",
                ProviderId = 12345678,
                Confirmed = false
            };

            // Act
            var result = await _sut.ProviderConfirm(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            result.RouteValues["encodedAccountId"].Should().Be(vm.EncodedAccountId);
        }

        [Test]
        public async Task ProviderConfirm_POST_WhenYesSelected_AndAttributesMissing_ShouldRedirectToError()
        {
            // Arrange
            var vm = new ProviderConfirmViewModel
            {
                EncodedAccountId = "ABC123",
                AccountId = 98765,
                ProviderId = 12345678,
                ProviderName = "Acme Training",
                Confirmed = true
            };

            _outerApi.Setup(o => o.GetAllQuestionAttributes()).ReturnsAsync((IEnumerable<QuestionAttribute>)null);

            // Act
            var result = await _sut.ProviderConfirm(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result!.RouteName.Should().Be(ErrorController.ErrorGet);
        }

        [Test]
        public async Task ProviderConfirm_POST_WhenYesSelected_ShouldCreateSurveyModel_AndRedirectToStart()
        {
            // Arrange
            var vm = new ProviderConfirmViewModel
            {
                EncodedAccountId = "ABC123",
                AccountId = 98765,
                ProviderId = 12345678,
                ProviderName = "Acme Training",
                Confirmed = true
            };

            var attributes = new[]
            {
                new QuestionAttribute { AttributeName = "Comm" },
                new QuestionAttribute { AttributeName = "Quality" }
            };
            _outerApi.Setup(o => o.GetAllQuestionAttributes()).ReturnsAsync(attributes);

            _session.Setup(s => s.GetFeedbackSource(_userId)).ReturnsAsync(FeedbackSource.AdHoc);

            SurveyModel capturedSurvey = null;
            _session
                .Setup(s => s.SetSurveyModel(_userId, It.IsAny<SurveyModel>()))
                .Callback<Guid, SurveyModel>((_, m) => capturedSurvey = m)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.ProviderConfirm(vm) as RedirectToRouteResult;

            // Assert
            result.Should().NotBeNull();
            result.RouteName.Should().Be(QuestionsController.StartFeedbackGet);
            result.RouteValues!["EncodedAccountId"].Should().Be(vm.EncodedAccountId);

            capturedSurvey.Should().NotBeNull();
            capturedSurvey.AccountId.Should().Be(vm.AccountId);
            capturedSurvey.EncodedAccountId.Should().Be(vm.EncodedAccountId);
            capturedSurvey.Ukprn.Should().Be(vm.ProviderId);
            capturedSurvey.UserRef.Should().Be(_userId);
            capturedSurvey.ProviderName.Should().Be(vm.ProviderName);
            capturedSurvey.Attributes.Select(a => a.Name).Should().BeEquivalentTo("Comm", "Quality");
            capturedSurvey.FeedbackSource.Should().Be(FeedbackSource.AdHoc);

            _session.Verify(s => s.SetSurveyModel(_userId, It.IsAny<SurveyModel>()), Times.Once);
        }
    }
}
