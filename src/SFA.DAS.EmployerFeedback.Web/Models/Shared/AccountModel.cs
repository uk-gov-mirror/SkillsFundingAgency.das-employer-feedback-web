using SFA.DAS.EmployerFeedback.Web.Attributes;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    public class AccountModel
    {
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), Encoding.EncodingType.AccountId)]
        public long AccountId { get; set; }
    }
}
