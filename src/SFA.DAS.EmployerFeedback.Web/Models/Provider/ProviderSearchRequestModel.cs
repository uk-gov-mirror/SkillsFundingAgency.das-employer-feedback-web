using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Provider
{
    public class ProviderSearchRequestModel : AccountModel
    {
        public FeedbackSource FeedbackSource { get; set; } = FeedbackSource.AdHoc;
    }
}
