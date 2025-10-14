using System;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class Feedback
    {
        public long FeedbackSource { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
    }
}