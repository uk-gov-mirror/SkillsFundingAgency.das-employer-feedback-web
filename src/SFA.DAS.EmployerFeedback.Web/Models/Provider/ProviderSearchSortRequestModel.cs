using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Provider
{
    public class ProviderSearchSortRequestModel : AccountModel
    {
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }
}
