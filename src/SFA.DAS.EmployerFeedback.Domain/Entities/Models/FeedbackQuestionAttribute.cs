using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [ExcludeFromCodeCoverage]
    public class FeedbackQuestionAttribute
    {
        public long AttributeId { get; set; }
        public string AttributeName { get; set; }
        public ICollection<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
