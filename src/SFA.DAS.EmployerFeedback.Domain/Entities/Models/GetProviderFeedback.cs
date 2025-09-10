using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class GetProviderFeedback
    {
        public Guid AccountID { get; set; }

        public string AccountName { get; set; }

        public List<ProviderFeedback> providers { get; set; } = new List<ProviderFeedback>();
    }
}
