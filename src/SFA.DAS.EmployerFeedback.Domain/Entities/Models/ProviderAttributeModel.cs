using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [ExcludeFromCodeCoverage]
    public class ProviderAttribute
    {
        public long AttributeId { get; set; }
        public int AttributeValue { get; set; }
    }
}
