using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Web.Extensions;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;

namespace SFA.DAS.EmployerProvideFeedback.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        public ReviewAnswersOrchestrator(IEmployerFeedbackOuterApi employerFeedbackOuterApi,ILogger<ReviewAnswersOrchestrator> logger)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
        }

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel)
        {

            var employerFeedback = await _employerFeedbackOuterApi.GetEmployerFeedbackRecord(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            long feedbackId = 0;
            if (null == employerFeedback)
            {
                feedbackId = await _employerFeedbackOuterApi.UpsertIntoFeedback(surveyModel.UserRef, surveyModel.AccountId, surveyModel.Ukprn);
            }
            else
            {
                feedbackId = employerFeedback.FeedbackId;
            }

            if (feedbackId == default(long))
            {
                throw new InvalidOperationException($"Unable to find or create feedback record");
            }

            try
            {
                var providerAttributes = await ConvertSurveyToProviderAttributes(surveyModel);

                var feedbackSource = FeedbackSource.AdHoc;
                if (surveyModel.UniqueCode.HasValue)
                {
                    feedbackSource = FeedbackSource.Email;
                }

                EmployerFeedbackResult employerFeedbackResult = new EmployerFeedbackResult {
                    FeedbackId = feedbackId,
                    ProviderRating = surveyModel.Rating.Value.GetDisplayName(),
                    SubmittedDate = DateTime.UtcNow,
                    FeedbackSource = feedbackSource,
                    ProviderAttributes = providerAttributes.ToList()
                };

                var employerFeedbackResultId =
                    await _employerFeedbackOuterApi.SubmitEmployerFeedback(employerFeedbackResult);
                if (null != surveyModel.UniqueCode && surveyModel.UniqueCode.HasValue)
                {
                    // Email journey.
                    await _employerFeedbackOuterApi.SetCodeBurntDate(surveyModel.UniqueCode.Value);
                }
                else
                {
                    // Ad Hoc journey
                    Guid? uniqueSurveyCode = await _employerFeedbackOuterApi.GetUniqueSurveyCodeFromFeedbackId(feedbackId);
                    if (uniqueSurveyCode != Guid.Empty)
                        await _employerFeedbackOuterApi.SetCodeBurntDate(uniqueSurveyCode.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit feedback");
            }
        }

        private async Task<IEnumerable<ProviderAttribute>> ConvertSurveyToProviderAttributes(SurveyModel surveyModel)
        {
            var feedbackQuestionAttributes = await _employerFeedbackOuterApi.GetAllAttributes();
            var providerAttributes = new List<ProviderAttribute>();

            foreach (var attribute in surveyModel.Attributes.Where(s => s.Good || s.Bad))
            {
                var providerAttribute = feedbackQuestionAttributes.FirstOrDefault(s => s.AttributeName == attribute.Name);
                if (providerAttribute != null)
                {
                    providerAttributes.Add(new ProviderAttribute
                    {
                        AttributeId = providerAttribute.AttributeId,
                        AttributeValue = attribute.Score,
                    });
                }
            }

            return providerAttributes;
        }
    }
}
