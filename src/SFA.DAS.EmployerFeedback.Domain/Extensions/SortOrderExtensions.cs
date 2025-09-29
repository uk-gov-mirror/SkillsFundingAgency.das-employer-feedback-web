using SFA.DAS.EmployerFeedback.Domain.Types;

namespace SFA.DAS.EmployerFeedback.Domain.Extensions
{
    public static class SortOrderExtensions
    {
        public static SortOrder Reverse(this SortOrder sortOrder)
        {
            return sortOrder == SortOrder.Ascending
                ? SortOrder.Descending
                : SortOrder.Ascending;
        }
    }
}
