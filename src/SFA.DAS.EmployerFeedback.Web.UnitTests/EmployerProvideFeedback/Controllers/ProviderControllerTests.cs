using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.ViewModels;
using SFA.DAS.EmployerProvideFeedback.Paging;
using SFA.DAS.EmployerProvideFeedback.Services;
using SFA.DAS.Encoding;

namespace UnitTests.EmployerProvideFeedback.Controllers
{
    public class ProviderControllerTests
    {
        private ProviderController _controller;
        private Mock<ISessionStorageService> _sessionServiceMock;
        private Mock<ITrainingProviderService> _trainingProviderServiceMock;
        private Mock<IEncodingService> _encodingServiceMock;
        private Mock<ILogger<ProviderController>> _loggerMock;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApiMock;
        private SurveyModel _surveyModel;
        private UrlBuilder _urlBuilder;

        [SetUp]
        public void SetUp()
        {
            _surveyModel = new SurveyModel
            {
                UserRef = Guid.NewGuid(),
                ProviderName = "TestProviderName",
            };

            _sessionServiceMock = new Mock<ISessionStorageService>();
            _sessionServiceMock
                .Setup(mock => mock.Get<SurveyModel>(It.IsAny<string>()))
                .ReturnsAsync(_surveyModel);

            _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
            _trainingProviderServiceMock
                .Setup(m => m.GetTrainingProviderSearchViewModel(
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

            _encodingServiceMock = new Mock<IEncodingService>();
            _loggerMock = new Mock<ILogger<ProviderController>>();
            _urlBuilder = new UrlBuilder("LOCAL");
            _employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();

            _controller = new ProviderController(
                _sessionServiceMock.Object,
                _trainingProviderServiceMock.Object,
                _encodingServiceMock.Object,
                _loggerMock.Object,
                _employerFeedbackOuterApiMock.Object,
                _urlBuilder);

            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(EmployerClaims.UserId, "TestUserIdValue"),
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
            
        }

        [TearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }
    }
}
