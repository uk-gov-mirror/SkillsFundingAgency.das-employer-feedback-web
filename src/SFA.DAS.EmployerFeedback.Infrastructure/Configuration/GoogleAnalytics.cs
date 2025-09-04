using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class GoogleAnalytics
    {
        public string TrackingId { get; set; }
        public string GoogleTagManagerId { get; set; }
    }
}