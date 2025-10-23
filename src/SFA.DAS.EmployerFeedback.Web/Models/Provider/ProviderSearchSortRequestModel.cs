using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Provider
{
    public class ProviderSearchSortRequestModel : AccountModel
    {
        public SortColumn SortColumn { get; set; }
        public SortOrder SortOrder { get; set; }
    }
}
