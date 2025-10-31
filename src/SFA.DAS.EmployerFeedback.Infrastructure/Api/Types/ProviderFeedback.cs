namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Types
{
    public class ProviderFeedback
    {
        public long Ukprn { get; set; }
        public string ProviderName { get; set; }
        public Feedback Feedback { get; set; }
        public bool HasNewStart { get; set; }
        public bool HasActive { get; set; }
        public bool HasCompleted { get; set; }

    }
}
