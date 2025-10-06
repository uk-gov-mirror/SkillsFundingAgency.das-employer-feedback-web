using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [ExcludeFromCodeCoverage]
    public class GetProviderFeedback
    {
        public long AccountId { get; set; }

        public string AccountName { get; set; }

        public List<ProviderFeedback> Providers { get; set; } = new List<ProviderFeedback>();
    }
}
