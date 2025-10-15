using SFA.DAS.EmployerFeedback.Domain.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResult
    {
        public Guid UserRef { get; set; }
        public long AccountId { get; set; }
        public long Ukprn { get; set; }
        public FeedbackSource FeedbackSource { get; set; }
        public string ProviderRating { get; set; }
        public IEnumerable<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
