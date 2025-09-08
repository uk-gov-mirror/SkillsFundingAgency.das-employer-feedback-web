using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [Serializable]
    public class EmployerFeedbackStarsAnnualSummaryDto
    {
        [JsonProperty(PropertyName = "ukprn", Order = int.MinValue)]
        public long Ukprn { get; set; }

        [JsonProperty(PropertyName = "stars", Order = int.MinValue)]
        public int Stars { get; set; }

        [JsonProperty(PropertyName = "reviewCount", Order = int.MinValue)]
        public int ReviewCount { get; set; }

        [JsonProperty(PropertyName = "timeperiod")]
        public string TimePeriod { get; set; }

        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeAnnualSummaryItemDto> ProviderAttribute { get; set; }
    }
}
