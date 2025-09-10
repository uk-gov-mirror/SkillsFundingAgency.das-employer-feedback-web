using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [ExcludeFromCodeCoverage]
    public class Feedback
    {
        public long FeedbackSource { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
    }
}