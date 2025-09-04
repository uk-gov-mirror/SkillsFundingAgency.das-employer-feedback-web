using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFeedback.Web.Models
{
    public class Parameters
    {
        [FromRoute]
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), EncodingType.AccountId)]
        public long AccountId { get; set; }
    }
}
