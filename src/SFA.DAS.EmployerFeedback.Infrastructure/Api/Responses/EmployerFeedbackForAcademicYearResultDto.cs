
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [Serializable]
    public class EmployerFeedbackForAcademicYearResultDto : EmployerFeedbackStarsSummaryDto
    {
        [JsonProperty(PropertyName = "providerAttribute")]
        public IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto> ProviderAttribute { get; set; }
    }
}
