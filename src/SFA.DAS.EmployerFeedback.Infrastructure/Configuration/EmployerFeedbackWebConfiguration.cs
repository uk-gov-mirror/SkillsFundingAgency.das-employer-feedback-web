using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class EmployerFeedbackWebConfiguration
    {
        public string FindApprenticeshipTrainingBaseUrl { get; set; }
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
        public int SlidingExpirationMinutes { get; set; }
        public int FeedbackWaitPeriodDays { get; set; }
        public string ZendeskSectionId { get; set; }

        // FIXME - public GoogleAnalyticsConfiguration GoogleAnalytics { get; set; }
        
        public ExternalLinksConfiguration ExternalLinks { get; set; }
    }
}