using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Paging;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    public class ProviderControllerTests
    {
        private ProviderController _controller;
        private Mock<ISessionStorageService> _sessionServiceMock;
        private Mock<ITrainingProviderService> _trainingProviderServiceMock;
        private Mock<ILogger<ProviderController>> _loggerMock;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApiMock;
        private Mock<IUserService> _userServiceMock;
        private SurveyModel _surveyModel;
        private UrlBuilder _urlBuilder;

        [SetUp]
        public void SetUp()
        {
            _surveyModel = new SurveyModel
            {
                UserRef = Guid.NewGuid(),
                ProviderName = "TestProviderName",
                Ukprn = 10000001,
                AccountId = 123456,
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel
                    {
                        Name = "TestAttribute1",
                        Good = true,
                        Bad = false
                    },
                    new ProviderAttributeModel
                    {
                        Name = "TestAttribute2",
                        Good = false,
                        Bad = true
                    }
                },
                FeedbackSource = Domain.Types.FeedbackSource.AdHoc,
            };

            _userServiceMock = new Mock<IUserService>();
            _userServiceMock.Setup(m => m.GetUserId()).Returns(new Guid().ToString());

            _sessionServiceMock = new Mock<ISessionStorageService>();
            _sessionServiceMock
                .Setup(mock => mock.GetSurveyModel(It.IsAny<string>()))
                .ReturnsAsync(_surveyModel);

            _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
            _trainingProviderServiceMock
                .Setup(m => m.GetTrainingProviderSearchViewModel(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ReturnsAsync(new ProviderSearchViewModel
                {
                    TrainingProviders = new PaginatedList<ProviderSearchViewModel.EmployerTrainingProvider>(
                        new List<ProviderSearchViewModel.EmployerTrainingProvider>(), 0, 0, 0, 6)
                });

            _loggerMock = new Mock<ILogger<ProviderController>>();
            _urlBuilder = new UrlBuilder("LOCAL");
            _employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();

            _controller = new ProviderController(
                _sessionServiceMock.Object,
                _trainingProviderServiceMock.Object,
                _loggerMock.Object,
                _employerFeedbackOuterApiMock.Object,
                _urlBuilder,
                _userServiceMock.Object);

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(EmployerClaims.UserId, new Guid().ToString()),
                }))
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        public class Index : ProviderControllerTests
        {
            [Test]
            public async Task Valid_AccountId_Should_Return_View()
            {
                // Arrange
                var request = new GetProvidersForFeedbackRequest();

                // Act
                var result = await _controller.Index(request);

                // Assert
                result.Should().BeOfType<ViewResult>();
                _controller.ViewData.Should().HaveCount(1);
            }

            [Test]
            public async Task Filter_Should_Return_View()
            {
                // Arrange
                var request = new ProviderSearchViewModel
                {
                    SelectedProviderName = "Test",
                    SelectedFeedbackStatus = "All",
                    SortColumn = "ProviderName",
                    SortDirection = "Asc"
                };

                _trainingProviderServiceMock.Setup(m => m.GetTrainingProviderSearchViewModel(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())).ReturnsAsync(request);

                // Act
                var result = await _controller.Filter(request);

                // Assert
                result.Should().BeOfType<ViewResult>();
                _controller.ViewData.Should().HaveCount(1);
            }


            [Test]
            public async Task Filter_PagingStateExists_Should_Return_View()
            {
                // Arrange
                var request = new ProviderSearchViewModel
                {
                    SelectedProviderName = "Test",
                    SelectedFeedbackStatus = "All",
                    SortColumn = "ProviderName",
                    SortDirection = "Asc"
                };

                _sessionServiceMock.Setup(x => x.GetPagingState(It.IsAny<string>()))
                    .ReturnsAsync(new PagingState
                    {
                        PageIndex = 1,
                        PageSize = 6,
                        SortColumn = "ProviderName",
                        SortDirection = "Asc",
                        SelectedFeedbackStatus = "All",
                        SelectedProviderName = "Test"
                    });

                _trainingProviderServiceMock.Setup(m => m.GetTrainingProviderSearchViewModel(
                    It.IsAny<long>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>())).ReturnsAsync(request);

                // Act
                var result = await _controller.Filter(request);

                // Assert
                result.Should().BeOfType<ViewResult>();
                _controller.ViewData.Should().HaveCount(1);
            }

            [Test]
            public async Task SortProviders_ShouldRedirectToIndex()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                string sortColumn = "ProviderName";
                string sortDirection = "Asc";

                // Act
                var result = await _controller.SortProviders(encodedAccountId, sortColumn, sortDirection);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be(nameof(_controller.Index));
            }

            [Test]
            public async Task ClearFilters_ShouldRedirectToIndex()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";

                // Act
                var result = await _controller.ClearFilters(encodedAccountId);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be(nameof(_controller.Index));
            }

            [Test]
            public async Task ConfirmProvider_ShoulReturnViewWithModel()
            {
                // Arrange
                var providerSearchModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = "ENCODED123",
                    ProviderId = 10000001,
                    ProviderName = "Test Provider",
                    Confirmed = true
                };


                _trainingProviderServiceMock.Setup(m => m.GetTrainingProviderConfirmationViewModel(
                        It.IsAny<long>(),
                        It.IsAny<Guid>(),
                        It.IsAny<long>()))
                    .ReturnsAsync(providerSearchModel);


                // Act
                var result = await _controller.ConfirmProvider(providerSearchModel);

                // Assert
                result.Should().BeOfType<ViewResult>();
                _controller.ViewData.Model.Should().NotBeNull();

                var providerSearchedConfirmedViewModel = _controller.ViewData.Model as ProviderSearchConfirmationViewModel;
                providerSearchedConfirmedViewModel.Should().NotBeNull();
                providerSearchedConfirmedViewModel.Should().BeEquivalentTo(providerSearchModel);
            }

            [Test]
            public async Task ProviderConfirmed_RedirecToLandingPage()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                long ukprn = 10000001;

                var postedModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = encodedAccountId,
                    ProviderId = ukprn,
                    ProviderName = "Test Provider",
                    Confirmed = true
                };

                // Ensure training provider service returns something reasonable if the controller calls it
                _trainingProviderServiceMock.Setup(m => m.GetTrainingProviderConfirmationViewModel(
                        It.IsAny<long>(),
                        It.IsAny<Guid>(),
                        It.IsAny<long>()))
                    .ReturnsAsync(new ProviderSearchConfirmationViewModel
                    {
                        EncodedAccountId = encodedAccountId,
                        ProviderId = ukprn,
                        ProviderName = "Test Provider",
                        Confirmed = true
                    });

                // Act
                var result = await _controller.ProviderConfirmed(postedModel);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be("StartFeedback");

                var surveyModel = _sessionServiceMock.Object.GetSurveyModel("TEST_USER_ID").Result;
                surveyModel.Should().BeEquivalentTo(_surveyModel);
            }

            [Test]
            public async Task ProviderConfirmed_NullConfirmedProvider_ShouldRedirectToConfirmProvider()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                long ukprn = 10000001;

                var postedModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = encodedAccountId,
                    ProviderId = ukprn,
                    ProviderName = "Test Provider",
                    Confirmed = null
                };

                // Act
                var result = await _controller.ProviderConfirmed(postedModel);

                // Assert
                result.Should().BeOfType<ViewResult>();
                _controller.ViewData.Model.Should().Be(postedModel);
            }

            [Test]
            public async Task ProviderConfirmed_DoesNotHaveConfirmedValue_ShouldRedirectToIndex()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                long ukprn = 10000001;

                var postedModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = encodedAccountId,
                    ProviderId = ukprn,
                    ProviderName = "Test Provider",
                    Confirmed = false
                };

                _controller.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, encodedAccountId);
                _controller.Request.RouteValues.Add(RouteValueKeys.ProviderId, ukprn);

                // Act
                var result = await _controller.ProviderConfirmed(postedModel);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be("Index");
                redirectResult.RouteValues.Should().ContainKey("encodedAccountId");
                redirectResult.RouteValues["encodedAccountId"].Should().Be(encodedAccountId);
                redirectResult.RouteValues.Should().ContainKey("providerId");
                redirectResult.RouteValues["providerId"].Should().Be(ukprn);
            }

            [Test]
            public async Task ProviderConfirmed_AttributesError_RedirectToError()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                long ukprn = 10000001;
                var postedModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = encodedAccountId,
                    ProviderId = ukprn,
                    ProviderName = "Test Provider",
                    Confirmed = true
                };

                _employerFeedbackOuterApiMock.Setup(m => m.GetAllAttributes()).ReturnsAsync((List<FeedbackQuestionAttribute>)null);

                // Act
                var result = await _controller.ProviderConfirmed(postedModel);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be("Error");
                redirectResult.ControllerName.Should().Be("Error");
            }

            [Test]
            public async Task ProviderConfirmed_UserCannotBeFound_RedirectToError()
            {
                // Arrange
                string encodedAccountId = "ENCODED123";
                long ukprn = 10000001;
                var postedModel = new ProviderSearchConfirmationViewModel
                {
                    EncodedAccountId = encodedAccountId,
                    ProviderId = ukprn,
                    ProviderName = "Test Provider",
                    Confirmed = true
                };

                _userServiceMock.Setup(m => m.GetUserId()).Returns((string)null);

                // Act
                var result = await _controller.ProviderConfirmed(postedModel);

                // Assert
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = (RedirectToActionResult)result;
                redirectResult.ActionName.Should().Be("Error");
                redirectResult.ControllerName.Should().Be("Error");
            }

            [Test]
            public async Task SessionSurvey_DoesNotExist_ShouldRedirectToNotFoundError()
            {
                //  Arrange
                _sessionServiceMock.Setup(mock => mock.GetSurveyModel(It.IsAny<string>())).ReturnsAsync((SurveyModel)null);

                // Act
                var result = await _controller.StartFeedback();

                // Assert
                result.Should().BeOfType<NotFoundResult>();
            }
        }

        [TearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }
    }
}
