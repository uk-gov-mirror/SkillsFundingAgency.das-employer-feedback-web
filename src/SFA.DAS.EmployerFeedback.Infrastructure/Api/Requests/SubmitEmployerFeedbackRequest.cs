using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests
{
    public class SubmitEmployerFeedbackRequest
    {
        public Guid UserRef { get; set; }
        public long AccountId { get; set; }
        public long Ukprn { get; set; }
        public FeedbackSource FeedbackSource { get; set; }
        public string ProviderRating { get; set; }
        public IEnumerable<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
