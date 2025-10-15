using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [ExcludeFromCodeCoverage]
    public class EmployerIdentifier
    {
        [JsonProperty("encodedAccountId")]
        public string AccountId { get; set; }
        [JsonProperty("dasAccountName")]
        public string EmployerName { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("apprenticeshipEmployerType")]
        public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
    }
}