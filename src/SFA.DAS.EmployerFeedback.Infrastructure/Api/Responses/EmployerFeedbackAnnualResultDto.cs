
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [Serializable]
    public class EmployerFeedbackAnnualResultDto 
    {
        public IEnumerable<EmployerFeedbackStarsAnnualSummaryDto> AnnualEmployerFeedbackDetails { get; set; }
   }
}
