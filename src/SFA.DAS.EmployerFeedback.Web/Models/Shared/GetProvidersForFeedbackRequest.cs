using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Attributes;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmployerFeedback.Web.ViewModels
{
    public class GetProvidersForFeedbackRequest
    {
        public string EncodedAccountId { get; set; }

        [AutoDecode(nameof(EncodedAccountId), EncodingType.AccountId)]
        public long AccountId { get; set; }

        public FeedbackSource FeedbackSource { get; set; } = FeedbackSource.AdHoc;

    }
}
