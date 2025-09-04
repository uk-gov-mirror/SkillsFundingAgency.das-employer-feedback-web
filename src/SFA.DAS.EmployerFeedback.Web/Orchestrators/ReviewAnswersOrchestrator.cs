using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;

        public ReviewAnswersOrchestrator(ILogger<ReviewAnswersOrchestrator> logger)
        {
            _logger = logger;
        }

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel)
        {
            //FIXME - change repoisitory with outer API call
            //var employerFeedback = await _employerFeedbackRepository.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            //long feedbackId = 0;
            //if(null == employerFeedback)
            //{
            //    feedbackId = await _employerFeedbackRepository.UpsertIntoFeedback(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            //}
            //else
            //{
            //    feedbackId = employerFeedback.FeedbackId;
            //}

            //if (feedbackId == default(long))
            //{
            //    throw new InvalidOperationException($"Unable to find or create feedback record");
            //}

            //try
            //{
            //    var providerAttributes = await ConvertSurveyToProviderAttributes(surveyModel);

            //    var feedbackSource = ProvideFeedback.Data.Enums.FeedbackSource.AdHoc;
            //    if(surveyModel.UniqueCode.HasValue)
            //    {
            //        feedbackSource = ProvideFeedback.Data.Enums.FeedbackSource.Email;
            //    }

            //    var employerFeedbackResultId =
            //        await _employerFeedbackRepository.CreateEmployerFeedbackResult(
            //        feedbackId,
            //        surveyModel.Rating.Value.GetDisplayName(),
            //        DateTime.UtcNow,
            //        feedbackSource,
            //        providerAttributes);

            //    if(null != surveyModel.UniqueCode && surveyModel.UniqueCode.HasValue)
            //    {
            //        // Email journey.
            //        await _employerFeedbackRepository.SetCodeBurntDate(surveyModel.UniqueCode.Value);
            //    }
            //    else
            //    {
            //        // Ad Hoc journey
            //        Guid? uniqueSurveyCode = await _employerFeedbackRepository.GetUniqueSurveyCodeFromFeedbackId(feedbackId);
            //        if (uniqueSurveyCode != Guid.Empty)
            //            await _employerFeedbackRepository.SetCodeBurntDate(uniqueSurveyCode.Value);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Failed to submit feedback");
            //}
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<ProviderAttributeModel>> ConvertSurveyToProviderAttributes(SurveyModel surveyModel)
        {
            //FIXME - change repoisitory with outer API call
            //var feedbackQuestionAttributes = await _employerFeedbackRepository.GetAllAttributes();
            //var providerAttributes = new List<ProviderAttribute>();

            //foreach (var attribute in surveyModel.Attributes.Where(s => s.Good || s.Bad))
            //{
            //    var providerAttribute = feedbackQuestionAttributes.FirstOrDefault(s => s.AttributeName == attribute.Name);
            //    if (providerAttribute != null)
            //    {
            //        providerAttributes.Add(new ProviderAttribute
            //        {
            //            AttributeId = providerAttribute.AttributeId,
            //            AttributeValue = attribute.Score,
            //        });
            //    }
            //}

            //return providerAttributes;
            throw new NotImplementedException();
        }
    }
}
