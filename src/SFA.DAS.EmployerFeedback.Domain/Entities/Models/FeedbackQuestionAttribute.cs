using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class FeedbackQuestionAttribute
    {
        public long AttributeId { get; set; }
        public string AttributeName { get; set; }
        public ICollection<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
