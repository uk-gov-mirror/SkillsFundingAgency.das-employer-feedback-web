using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class ReviewAnswersOrchestratorTests
    {
        private ReviewAnswersOrchestrator _orchestrator;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _logger;
        private Mock<EmployerFeedbackResult> _employerFeedback;

        [SetUp]
        public void SetUp()
        {
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _orchestrator = new ReviewAnswersOrchestrator(_employerFeedbackOuterApi.Object, _logger.Object);

            _employerFeedback = new Mock<EmployerFeedbackResult>();
        }

        [Test, AutoData]
        public async Task WhenUsingEmailJourney_ThenDateCompletedIsSet(SurveyModel surveyModel)
        {
            // Arrange
            var employerFeedbackAndResult = new List<EmployerFeedbackAndResult>()
            {
                new EmployerFeedbackAndResult {
                    AccountId = 1,
                    FeedbackId = 1,
                    IsActive = true,
                    UserRef = new Guid(),
                    DateTimeCompleted = DateTime.Now
                }
            };

            _employerFeedbackOuterApi.Setup(x =>
                x.SubmitEmployerFeedback(_employerFeedback.Object))
                .ReturnsAsync(employerFeedbackAndResult);

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackOuterApi.Verify(x => x.SubmitEmployerFeedback(It.IsAny<EmployerFeedbackResult>()), Times.Once);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_ThenDateCompletedIsSet(SurveyModel surveyModel)
        {
            // Arrange
            var employerFeedbackAndResult = new List<EmployerFeedbackAndResult>()
            {
                new EmployerFeedbackAndResult {
                    AccountId = 1,
                    FeedbackId = 1,
                    IsActive = true,
                    UserRef = new Guid(),
                    DateTimeCompleted = DateTime.Now
                }
            };

            _employerFeedbackOuterApi.Setup(x =>
                x.SubmitEmployerFeedback(_employerFeedback.Object))
                .ReturnsAsync(employerFeedbackAndResult);

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackOuterApi.Verify(x => x.SubmitEmployerFeedback(It.IsAny<EmployerFeedbackResult>()), Times.Once);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_AndNoSurveyInvite_NoBurnDateSet(SurveyModel surveyModel)
        {
            // Arrange
            var employerFeedbackAndResult = new List<EmployerFeedbackAndResult>()
            {
                new EmployerFeedbackAndResult {
                    AccountId = 1,
                    FeedbackId = 1,
                    IsActive = true,
                    UserRef = new Guid(),
                    DateTimeCompleted = DateTime.Now
                }
            };

            _employerFeedbackOuterApi.Setup(x =>
                x.SubmitEmployerFeedback(_employerFeedback.Object))
                .ReturnsAsync(employerFeedbackAndResult);

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackOuterApi.Verify(x => x.SubmitEmployerFeedback(_employerFeedback.Object), Times.Never);
        }
    }
}
