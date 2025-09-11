
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [Serializable]
    public class EmployerFeedbackAnnualResultDto 
    {
        public IEnumerable<EmployerFeedbackStarsAnnualSummaryDto> AnnualEmployerFeedbackDetails { get; set; }
   }
}
