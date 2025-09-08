using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerProvideFeedback.Orchestrators;

namespace UnitTests.Orchestrators
{
    [TestFixture]
    public class ReviewAnswersOrchestratorTests
    {
        private ReviewAnswersOrchestrator _orchestrator;
        private Mock<ILogger<ReviewAnswersOrchestrator>> _logger;
        private Mock<EmployerFeedback> _employerFeedback;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _orchestrator = new ReviewAnswersOrchestrator(_logger.Object);
            _employerFeedback = new Mock<EmployerFeedback>();
        }
        

        [Test, AutoData]
        public async Task WhenUsingEmailJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            //FIXME - change repoisitory with outer API call
            //_employerFeedback.Object.FeedbackId = 1;
            //_employerFeedbackRepository.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
            //    .ReturnsAsync(_employerFeedback.Object);

            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //_employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            //_employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Never);
            throw new NotImplementedException();
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            //FIXME - change repoisitory with outer API call
            //// Arrange
            //surveyModel.UniqueCode = null;
            //_employerFeedback.Object.FeedbackId = 1;

            //_employerFeedbackRepository.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
            //    .ReturnsAsync(_employerFeedback.Object);

            //_employerFeedbackRepository.Setup(x =>
            //    x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
            //    .ReturnsAsync(Guid.NewGuid());

            //// Act
            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //// Assert
            //_employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            //_employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
            throw new NotImplementedException();
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_AndNoSurveyInvite_NoBurnDateSet(SurveyModel surveyModel)
        {
            //FIXME - change repoisitory with outer API call
            //// Arrange
            //surveyModel.UniqueCode = null;
            //_employerFeedback.Object.FeedbackId = 1;

            //_employerFeedbackRepository.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
            //    .ReturnsAsync(_employerFeedback.Object);

            //_employerFeedbackRepository.Setup(x =>
            //    x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
            //    .ReturnsAsync(Guid.Empty);

            //// Act
            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //// Assert
            //_employerFeedbackRepository.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Never);
            //_employerFeedbackRepository.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
            throw new NotImplementedException();
        }
    }
}
