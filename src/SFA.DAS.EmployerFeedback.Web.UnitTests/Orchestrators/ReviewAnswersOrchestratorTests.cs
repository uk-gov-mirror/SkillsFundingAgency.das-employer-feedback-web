using AutoFixture.NUnit3;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
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
        private Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<ReviewAnswersOrchestrator>>();
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _orchestrator = new ReviewAnswersOrchestrator(_employerFeedbackOuterApi.Object, _logger.Object);
            _employerFeedback = new Mock<EmployerFeedbackResponse>();
        }
        

        [Test, AutoData]
        public async Task WhenUsingEmailJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            throw new NotImplementedException();
            //_employerFeedback.Object.FeedbackId = 1;

            //_employerFeedbackOuterApi.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn   ))
            //    .ReturnsAsync(_employerFeedback.Object);

            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //_employerFeedbackOuterApi.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            //_employerFeedbackOuterApi.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Never);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_ThenBurnDateIsSet(SurveyModel surveyModel)
        {
            throw new NotImplementedException();
            //// Arrange
            //surveyModel.UniqueCode = null;
            //_employerFeedback.Object.FeedbackId = 1;

            //_employerFeedbackOuterApi.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
            //    .ReturnsAsync(_employerFeedback.Object);

            //_employerFeedbackOuterApi.Setup(x =>
            //    x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
            //    .ReturnsAsync(Guid.NewGuid());

            //// Act
            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //// Assert
            //_employerFeedbackOuterApi.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Once);
            //_employerFeedbackOuterApi.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
        }

        [Test, AutoData]
        public async Task WhenUsingAdHocJourney_AndNoSurveyInvite_NoBurnDateSet(SurveyModel surveyModel)
        {
            throw new NotImplementedException();
            // Arrange
            //surveyModel.UniqueCode = null;
            //_employerFeedback.Object.FeedbackId = 1;

            //_employerFeedbackOuterApi.Setup(x =>
            //    x.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn))
            //    .ReturnsAsync(_employerFeedback.Object);

            //_employerFeedbackOuterApi.Setup(x =>
            //    x.GetUniqueSurveyCodeFromFeedbackId(_employerFeedback.Object.FeedbackId))
            //    .ReturnsAsync(Guid.Empty);

            //// Act
            //await _orchestrator.SubmitConfirmedEmployerFeedback(surveyModel);

            //// Assert
            //_employerFeedbackOuterApi.Verify(x => x.SetCodeBurntDate(It.IsAny<Guid>()), Times.Never);
            //_employerFeedbackOuterApi.Verify(x => x.GetUniqueSurveyCodeFromFeedbackId(It.IsAny<long>()), Times.Once);
        }
    }
}
