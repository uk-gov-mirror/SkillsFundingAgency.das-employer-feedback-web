
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [Serializable]
    public class EmployerFeedbackResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeSummaryItemDto> ProviderAttribute { get; set; }
    }
}
