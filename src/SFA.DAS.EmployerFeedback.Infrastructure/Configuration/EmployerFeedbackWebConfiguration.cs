using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class EmployerFeedbackWebConfiguration
    {
        public string FindApprenticeshipTrainingBaseUrl { get; set; }
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
    }
}