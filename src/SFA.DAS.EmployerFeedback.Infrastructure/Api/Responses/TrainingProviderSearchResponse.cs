using System.Collections.Generic;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    public class TrainingProviderSearchResponse
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public List<ProviderFeedback> Providers { get; set; } = new List<ProviderFeedback>();
    }
}
