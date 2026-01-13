using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ProviderControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<ProviderController>> _mockLogger;
        private Mock<IProviderOrchestrator> _mockOrchestrator;
        private ProviderController _sut;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<ProviderController>>();
            _mockOrchestrator = new Mock<IProviderOrchestrator>();

            _sut = new ProviderController(_mockUserService.Object, _mockLogger.Object, _mockOrchestrator.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task ProviderSearch_Get_Should_Call_Orchestrator_And_Return_View()
        {
            // Arrange
            var request = new ProviderSearchRequestModel { EncodedAccountId = "ABC123", PageIndex = 2, FeedbackSource = FeedbackSource.Email };
            var viewModel = new ProviderSearchViewModel
            {
                Providers = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(new List<ProviderSearchViewModel.EmployerTrainingProvider>(), 0, 1, 10, 10)
            };
            _mockOrchestrator.Setup(o => o.GetProviderSearchViewModel(request)).ReturnsAsync(viewModel);

            // Act
            var result = await _sut.ProviderSearch(request);

            // Assert
            _mockOrchestrator.Verify(o => o.SetFeedbackSource(FeedbackSource.Email), Times.Once);
            _mockOrchestrator.Verify(o => o.SetProviderSearchPageIndex(2), Times.Once);
            _mockOrchestrator.Verify(o => o.GetProviderSearchViewModel(request), Times.Once);
            _mockOrchestrator.Verify(o => o.SetProviders(It.IsAny<List<ProviderSearchViewModel.EmployerTrainingProvider>>()), Times.Once);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(viewModel);
        }

        [Test]
        public async Task ProviderSearch_Post_Should_Update_Filters_And_Redirect()
        {
            // Arrange
            var viewModel = new ProviderSearchViewModel { EncodedAccountId = "XYZ" };

            // Act
            var result = await _sut.ProviderSearch(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateProviderSearchFilters(viewModel), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            redirect.RouteValues["EncodedAccountId"].Should().Be("XYZ");
        }

        [Test]
        public async Task SortProviders_Should_Update_Sort_And_Redirect()
        {
            // Arrange
            var model = new ProviderSearchSortRequestModel
            {
                EncodedAccountId = "ACC123",
                SortColumn = SortColumn.DateSubmitted,
                SortOrder = SortOrder.Ascending
            };

            // Act
            var result = await _sut.SortProviders(model);

            // Assert
            _mockOrchestrator.Verify(o => o.SortProviderSearch(model.SortColumn, model.SortOrder), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            redirect.RouteValues["EncodedAccountId"].Should().Be("ACC123");
        }

        [Test]
        public async Task ClearFilters_Should_Reset_State_And_Redirect()
        {
            // Arrange
            var model = new AccountModel { EncodedAccountId = "ID123" };

            // Act
            var result = await _sut.ClearFilters(model);

            // Assert
            _mockOrchestrator.Verify(o => o.ClearProviderSearchFilters(), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            redirect.RouteValues["EncodedAccountId"].Should().Be("ID123");
        }

        [Test]
        public async Task ProviderConfirm_Get_Should_Return_View_When_ViewModel_Found()
        {
            // Arrange
            var request = new ProviderConfirmRequestModel { ProviderId = 10, EncodedAccountId = "ABC123" };
            var confirmViewModel = new ProviderConfirmViewModel();
            _mockOrchestrator.Setup(o => o.GetProviderConfirmViewModel(request)).ReturnsAsync(confirmViewModel);

            // Act
            var result = await _sut.ProviderConfirm(request);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(confirmViewModel);
        }

        [Test]
        public async Task ProviderConfirm_Get_Should_Redirect_When_ViewModel_Null()
        {
            // Arrange
            var request = new ProviderConfirmRequestModel { ProviderId = 10, EncodedAccountId = "ACC123" };
            _mockOrchestrator.Setup(o => o.GetProviderConfirmViewModel(request)).ReturnsAsync((ProviderConfirmViewModel)null);

            // Act
            var result = await _sut.ProviderConfirm(request);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("ACC123");
        }

        [Test]
        public async Task ProviderConfirm_Post_Should_Redirect_To_ConfirmGet_When_Invalid()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel { ProviderId = 1, EncodedAccountId = "EFG123" };
            _mockOrchestrator.Setup(o => o.ValidateProviderConfirmViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.ProviderConfirm(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.ValidateProviderConfirmViewModel(viewModel, It.IsAny<ModelStateDictionary>()), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderConfirmGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG123");
        }

        [Test]
        public async Task ProviderConfirm_Post_Should_Redirect_To_Search_When_Not_Confirmed()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel { ProviderId = 1, EncodedAccountId = "EFG456", Confirmed = false };
            _mockOrchestrator.Setup(o => o.ValidateProviderConfirmViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ProviderConfirm(viewModel);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);
        }

        [Test]
        public async Task ProviderConfirm_Post_Should_Create_Survey_And_Redirect_To_StartFeedback_When_Confirmed()
        {
            // Arrange
            var viewModel = new ProviderConfirmViewModel { EncodedAccountId = "MNO789", Confirmed = true };
            _mockOrchestrator.Setup(o => o.ValidateProviderConfirmViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.ProviderConfirm(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.CreateNewSurvey(viewModel), Times.Once);

            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.StartFeedbackGet);
            redirect.RouteValues["EncodedAccountId"].Should().Be("MNO789");
        }

        [Test]
        public void ProviderSearch_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethods()
                .Single(m => m.Name == nameof(ProviderController.ProviderSearch) &&
                             m.GetParameters()[0].ParameterType == typeof(ProviderSearchRequestModel) &&
                             m.DeclaringType == typeof(ProviderController));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.ProviderSearchGet);
        }

        [Test]
        public void ProviderSearch_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethods()
                .Single(m => m.Name == nameof(ProviderController.ProviderSearch) &&
                             m.GetParameters()[0].ParameterType == typeof(ProviderSearchViewModel) &&
                             m.DeclaringType == typeof(ProviderController));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.ProviderSearchPost);
        }

        [Test]
        public void ProviderConfirm_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethods()
                .Single(m => m.Name == nameof(ProviderController.ProviderConfirm) &&
                             m.GetParameters()[0].ParameterType == typeof(ProviderConfirmRequestModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.ProviderConfirmGet);
        }

        [Test]
        public void ProviderConfirm_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethods()
                .Single(m => m.Name == nameof(ProviderController.ProviderConfirm) &&
                             m.GetParameters()[0].ParameterType == typeof(ProviderConfirmViewModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.ProviderConfirmPost);
        }

        [Test]
        public void SortProviders_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethod(nameof(ProviderController.SortProviders));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.SortProvidersGet);
        }

        [Test]
        public void ClearFilters_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ProviderController)
                .GetMethod(nameof(ProviderController.ClearFilters));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ProviderController.ClearFiltersGet);
        }
    }
}
