using System;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResponse
    {
        public long FeedbackId { get; set; }
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public bool IsActive { get; set; }
    }
}
