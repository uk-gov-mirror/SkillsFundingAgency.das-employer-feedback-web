using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests
{
    public class EmployerFeedbackRequest
    {
        public Guid UserRef { get; set; }

        public long Ukprn { get; set; }

        public long AccountId { get; set; }

    }
}
