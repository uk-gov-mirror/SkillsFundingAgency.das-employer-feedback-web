using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    using Newtonsoft.Json;

    /// <summary>
    /// A positive or negative attribute of a Provider
    /// </summary>
    [Serializable]
    public class ProviderAttributeDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}
