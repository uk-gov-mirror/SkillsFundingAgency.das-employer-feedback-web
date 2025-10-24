using SFA.DAS.Http.Configuration;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration
{
    public class EmployerFeedbackOuterApiConfiguration : IApimClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }
        public string ApiVersion { get; set; }
    }
}
