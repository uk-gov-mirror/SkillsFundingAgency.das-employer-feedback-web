using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Types
{
    public class Feedback
    {
        public long FeedbackSource { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
    }
}