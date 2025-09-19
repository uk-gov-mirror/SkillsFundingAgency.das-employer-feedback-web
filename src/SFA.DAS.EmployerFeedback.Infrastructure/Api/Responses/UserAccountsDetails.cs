using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{ 
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