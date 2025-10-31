using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    public class SurveyModel
    {
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public string EncodedAccountId { get; set; }  
        public List<ProviderAttributeModel> Attributes { get; set; } = new List<ProviderAttributeModel>();
        public ProviderRating? Rating { get; set; }
        public bool HasStrengths => Attributes.Any(attr => attr.Good);
        public bool HasWeaknesses => Attributes.Any(attr => attr.Bad);
        public string ProviderName { get; set; }
        public FeedbackSource FeedbackSource { get; set; }
    }
}