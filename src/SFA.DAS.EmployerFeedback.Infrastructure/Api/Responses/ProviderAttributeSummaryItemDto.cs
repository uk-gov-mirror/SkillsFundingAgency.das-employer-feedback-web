
using System;
using Newtonsoft.Json;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    /// <summary>
    /// Strength and Weakness counts of a Provider
    /// </summary>
    [Serializable]
    public class ProviderAttributeSummaryItemDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "strength")]
        public int Strength{ get; set; }

        [JsonProperty(PropertyName = "weakness")]
        public int Weakness { get; set; }
    }
}
