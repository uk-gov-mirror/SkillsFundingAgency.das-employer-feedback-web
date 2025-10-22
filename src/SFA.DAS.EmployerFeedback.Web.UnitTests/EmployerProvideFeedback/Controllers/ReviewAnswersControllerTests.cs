using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.EmployerFeedback.Controllers
{
    [TestFixture]
    public class ReviewAnswersControllerTests
    {
        private Mock<IUserService> _userService;
        private Mock<ILogger<ReviewAnswersController>> _controllerLoggerMock;
        private Mock<ISessionStorageService> _sessionService;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;
        private Mock<ITrainingProviderService> _trainingProviderServiceMock;
        private ReviewAnswersController _controller;
        private EmployerFeedbackWebConfiguration _config;
        private Guid _userId;
        

        [SetUp]
        public void Arrange()
        {
            _userId = Guid.NewGuid();
            _sessionService = new Mock<ISessionStorageService>();
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _trainingProviderServiceMock = new Mock<ITrainingProviderService>();
            Mock<IHttpContextAccessor> _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _userService = new Mock<IUserService>();
            _userService.Setup(m => m.GetUserId()).Returns(_userId);
            _controllerLoggerMock = new Mock<ILogger<ReviewAnswersController>>();

            _config = new EmployerFeedbackWebConfiguration();
            _config.FeedbackWaitPeriodDays = 21;
            _config.ExternalLinks = new ExternalLinksConfiguration();
            _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl = "https://findapprenticeshiptraining.sfa.gov.uk/";

            _controller = new ReviewAnswersController(_userService.Object, _controllerLoggerMock.Object, _sessionService.Object, _trainingProviderServiceMock.Object, _config);
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
            _trainingProviderServiceMock.Setup(s => s.CanSubmitFeedback(It.IsAny<SurveyModel>(), It.IsAny<Guid>())).ReturnsAsync(true);

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
