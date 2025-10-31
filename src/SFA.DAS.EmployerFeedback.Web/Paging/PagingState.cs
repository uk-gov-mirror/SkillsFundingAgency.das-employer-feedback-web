using SFA.DAS.EmployerFeedback.Domain.Types;

namespace SFA.DAS.EmployerFeedback.Web.Paging
{
    public class PagingState
    {
        public const int DefaultPageIndex = 1;
        public const int DefaultPageSize = 10;

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public SortOrder SortOrder { get; set; }
        public SortColumn SortColumn { get; set; }
        public string SelectedProviderName { get; set; }
        public string SelectedFeedbackStatus { get; set; }

        public PagingState()
        {
            PageIndex = DefaultPageIndex;
            PageSize = DefaultPageSize;
            SortColumn = SortColumn.Default;
            SortOrder = SortOrder.Ascending;
        }
    }
}
