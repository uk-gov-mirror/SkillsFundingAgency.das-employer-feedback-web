using SFA.DAS.EmployerFeedback.Web.Attributes;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    public class ProviderSearchConfirmationViewModel
    {
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), Encoding.EncodingType.AccountId)]
        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public bool? Confirmed { get; set; }
    }
}
