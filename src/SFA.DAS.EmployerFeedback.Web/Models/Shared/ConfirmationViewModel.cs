using SFA.DAS.EmployerFeedback.Domain.Types;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    public class ConfirmationViewModel
    {
        public string ProviderName { get; set; }

        public ProviderRating FeedbackRating { get; set; }

        public string FatUrl { get; internal set; }
        public string ComplaintToProviderSiteUrl { get; set; }
        public string ComplaintSiteUrl { get; set; }
        public string EmployerAccountsHomeUrl { get; set; }
        public bool HasMultipleProviders { get; set; }
        public string EncodedAccountId { get; set; }
    }
}
