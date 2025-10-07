using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [ExcludeFromCodeCoverage]
    public class EmployerFeedbackResponse
    {
        public long FeedbackId { get; set; }
        public Guid UserRef { get; set; }
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public bool IsActive { get; set; }
    }
}
