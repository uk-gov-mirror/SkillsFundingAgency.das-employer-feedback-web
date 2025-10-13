using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Web.Extensions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class ReviewAnswersOrchestrator
    {
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly ILogger<ReviewAnswersOrchestrator> _logger;

        public ReviewAnswersOrchestrator(IEmployerFeedbackOuterApi employerFeedbackOuterApi, ILogger<ReviewAnswersOrchestrator> logger)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
        }

        public async Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel)
        {
            try
            {
                await _employerFeedbackOuterApi.SubmitEmployerFeedback(new EmployerFeedbackResult
                {
                    Ukprn = surveyModel.Ukprn,
                    AccountId = surveyModel.AccountId,
                    ProviderRating = surveyModel.Rating.GetDisplayName(),
                    FeedbackSource = surveyModel.FeedbackSource,
                    ProviderAttributes = await ConvertSurveyToProviderAttributes(surveyModel),
                    UserRef = surveyModel.UserRef
                });
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
