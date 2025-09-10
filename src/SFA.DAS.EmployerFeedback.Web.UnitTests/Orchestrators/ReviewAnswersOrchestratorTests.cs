using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerProvideFeedback.Orchestrators;
using System;
using System.Threading.Tasks;

namespace UnitTests.Orchestrators
{
    [TestFixture]
    public class ReviewAnswersOrchestratorTests
    {
        private ReviewAnswersOrchestrator _orchestrator;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _logger;
        private Mock<EmployerFeedbackResponse> _employerFeedback;
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterAPI;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _employerFeedbackOuterAPI = new Mock<IEmployerFeedbackOuterApi>();
            _orchestrator = new ReviewAnswersOrchestrator(_employerFeedbackOuterAPI.Object, _logger.Object);
            _employerFeedback = new Mock<EmployerFeedbackResponse>();
        }
        

        [Test, AutoData]
        public async Task WhenUsingEmailJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            _employerFeedback.Object.FeedbackId = 1;
            var employerFeedbackRequest = new EmployerFeedbackRequest
            {
                Ukprn = surveyModel.Ukprn,
                AccountId = surveyModel.AccountId,
                UserRef = surveyModel.UserRef
            };
            
            _employerFeedbackOuterAPI.Setup(x =>
                x.GetEmployerFeedbackRecord(employerFeedbackRequest))
                .ReturnsAsync(_employerFeedback.Object);

            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            _employerFeedbackOuterAPI.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            _employerFeedbackOuterAPI.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Never);
            throw new NotImplementedException();
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            // Arrange
            surveyModel.UniqueCode = null;
            _employerFeedback.Object.FeedbackId = 1;

            var employerFeedbackRequest = new EmployerFeedbackRequest
            {
                Ukprn = surveyModel.Ukprn,
                AccountId = surveyModel.AccountId,
                UserRef = surveyModel.UserRef
            };

            _employerFeedbackOuterAPI.Setup(x =>
                x.GetEmployerFeedbackRecord(employerFeedbackRequest))
                .ReturnsAsync(_employerFeedback.Object);

            _employerFeedbackOuterAPI.Setup(x =>
                x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackOuterAPI.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            _employerFeedbackOuterAPI.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
            throw new NotImplementedException();
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_AndNoSurveyInvite_NoBurnDateSet(SurveyModel surveyModel)
        {
            // Arrange
            surveyModel.UniqueCode = null;
            _employerFeedback.Object.FeedbackId = 1;

            var employerFeedbackRequest = new EmployerFeedbackRequest
            {
                Ukprn = surveyModel.Ukprn,
                AccountId = surveyModel.AccountId,
                UserRef = surveyModel.UserRef
            };

            _employerFeedbackOuterAPI.Setup(x =>
                x.GetEmployerFeedbackRecord(employerFeedbackRequest))
                .ReturnsAsync(_employerFeedback.Object);

            _employerFeedbackOuterAPI.Setup(x =>
                x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
                .ReturnsAsync(Guid.Empty);

            // Act
            await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            // Assert
            _employerFeedbackOuterAPI.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Never);
            _employerFeedbackOuterAPI.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
            throw new NotImplementedException();
        }
    }
}
