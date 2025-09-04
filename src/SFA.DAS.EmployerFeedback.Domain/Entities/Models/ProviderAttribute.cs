using System;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class ProviderAttribute
    {
        public Guid EmployerFeedbackResultId { get; set; }
        public long AttributeId { get; set; }
        public int AttributeValue { get; set; }
    }
}
