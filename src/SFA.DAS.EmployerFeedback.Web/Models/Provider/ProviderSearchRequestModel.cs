using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Paging;

namespace SFA.DAS.EmployerFeedback.Web.Models.Provider
{
    public class ProviderSearchRequestModel : ProviderSearchSortRequestModel
    {
        public FeedbackSource FeedbackSource { get; set; } = FeedbackSource.AdHoc;
        public int PageIndex { get; set; } = PagingState.DefaultPageIndex;
    }
}
