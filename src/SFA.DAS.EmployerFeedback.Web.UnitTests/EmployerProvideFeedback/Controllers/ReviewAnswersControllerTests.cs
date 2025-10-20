using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.EmployerFeedback.Controllers
{
    [TestFixture]
    public class ReviewAnswersControllerTests
    {
        private Mock<IUserService> _userService;
        private Mock<ILogger<ReviewAnswersController>> _controllerLoggerMock;
        private Mock<ISessionStorageService> _sessionService;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _orchestratorLogger;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;
        private Mock<ITrainingProviderService> _trainingProviderServiceMock;
        private Mock<ReviewAnswersOrchestrator> _orchestrator;
        private ReviewAnswersController _controller;
        private EmployerFeedbackWebConfiguration _config;
        private Guid _userId;
        

        [SetUp]
        public void Arrange()
        {
            _userId = Guid.NewGuid();
            _sessionService = new Mock<ISessionStorageService>();
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _orchestratorLogger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
            Mock<IHttpContextAccessor> _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _userService = new Mock<IUserService>();
            _userService.Setup(m => m.GetUserId()).Returns(_userId);
            _controllerLoggerMock = new Mock<ILogger<ReviewAnswersController>>();

            _config = new EmployerFeedbackWebConfiguration();
            _config.FeedbackWaitPeriodDays = 21;
            _config.ExternalLinks = new ExternalLinksConfiguration();
            _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl = "https://findapprenticeshiptraining.sfa.gov.uk/";

            _orchestrator = new Mock<ReviewAnswersOrchestrator>(_employerFeedbackOuterApi.Object, _orchestratorLogger.Object, _sessionService.Object);
            _controller = new ReviewAnswersController(_sessionService.Object, _orchestrator.Object, _config, _employerFeedbackOuterApi.Object, _userService.Object, _controllerLoggerMock.Object, _trainingProviderServiceMock.Object);

            /*_controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                    new Claim(EmployerClaims.UserId, new Guid().ToString()){
                    },
                }))
                }
            };*/
        }

        [Test]
        public async Task Index_ReturnsView()
        {
            //Arrange
            _sessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(new SurveyModel());

            //Act
            var result = await _controller.Index();

            //Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task Confirmation_RedirectToConfirmationPage()
        {
            //Arrange
            var surveyModel = new SurveyModel
            {
                AccountId = 123,
                Ukprn = 456,
                ProviderName = "Test Provider",
                UserRef = Guid.NewGuid()
            };

            var providerFeedback = new GetProviderFeedback()
            {
                AccountId = 123,
                Providers = new List<ProviderFeedback>()
                {
                    new ProviderFeedback
                    {
                        Ukprn = 456,
                        ProviderName = "Test Provider",
                        Feedback = new Feedback()
                        {
                            FeedbackSource = (long)FeedbackSource.Email,
                            ProviderRating = ProviderRating.Good.ToString()
                        }
                    }
                }
            };

            _sessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(surveyModel);
            _employerFeedbackOuterApi
                .Setup(m => m.GetTrainingProviderSearch(It.IsAny<long>(), It.IsAny<Guid>()))
                .ReturnsAsync(providerFeedback);
            _trainingProviderServiceMock.Setup(s => s.CanSubmitFeedback(It.IsAny<DateTime?>())).Returns(true);

            //Act
            var result = await _controller.Confirmation();

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
            var redirectResult = (RedirectToRouteResult)result;
            Assert.That(redirectResult.RouteName, Is.EqualTo(ConfirmationController.ConfirmationGet));
        }

        [Test]
        public async Task Confirmation_FeedbackTooSoon_RedirectToFeedbackAlreadySubmitted()
        {
            //Arrange
            var surveyModel = new SurveyModel
            {
                AccountId = 123,
                Ukprn = 456,
                ProviderName = "Test Provider",
                UserRef = Guid.NewGuid()
            };

            var providerFeedback = new GetProviderFeedback()
            {
                AccountId = 123,
                Providers = new List<ProviderFeedback>()
                {
                    new ProviderFeedback
                    {
                        Ukprn = 456,
                        ProviderName = "Test Provider",
                        Feedback = new Feedback()
                        {
                            FeedbackSource = (long)FeedbackSource.Email,
                            ProviderRating = ProviderRating.Good.ToString(),
                            DateTimeCompleted = DateTime.UtcNow
                        },
                        HasCompleted = true
                    },

                }
            };

            _sessionService.Setup(s => s.GetSurveyModel(_userId)).ReturnsAsync(surveyModel);
            _employerFeedbackOuterApi
                .Setup(m => m.GetTrainingProviderSearch(It.IsAny<long>(), It.IsAny<Guid>()))
                .ReturnsAsync(providerFeedback);

            //Act
            var result = await _controller.Confirmation();

            //Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.TypeOf<RedirectToRouteResult>());
            var redirectResult = (RedirectToRouteResult)result;
            Assert.That(redirectResult.RouteName, Is.EqualTo(FeedbackSubmittedController.FeedbackAlreadySubmittedGet));
        }

        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
        }
    }
}
