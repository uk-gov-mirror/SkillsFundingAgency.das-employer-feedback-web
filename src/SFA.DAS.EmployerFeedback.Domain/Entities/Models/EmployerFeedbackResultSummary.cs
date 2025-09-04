using System;


namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResultSummary : ProviderStarsSummary
    {
        public string AttributeName { get; set; }
        public int Strength { get; set; }
        public int Weakness { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
