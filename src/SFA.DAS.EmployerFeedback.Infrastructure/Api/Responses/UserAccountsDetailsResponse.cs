using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    public class UserAccountsDetailsResponse
    {
        [JsonProperty("isSuspended")]
        public bool IsSuspended { get; set; }
        [JsonProperty("employerUserId")]
        public string EmployerUserId { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("userAccounts")]
        public List<EmployerIdentifier> UserAccounts { get; set; }
    }
}