using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class GetProviderFeedback
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public List<ProviderFeedback> Providers { get; set; } = new List<ProviderFeedback>();
    }
}
