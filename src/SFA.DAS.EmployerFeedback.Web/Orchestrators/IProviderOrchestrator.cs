using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public interface IProviderOrchestrator
    {
        Task<ProviderSearchViewModel> GetProviderSearchViewModel(ProviderSearchRequestModel model);
        Task SetProviderSearchPageIndex(int pageIndex);
        Task SetProviders(ProviderSearchViewModel viewModel);
        Task SetFeedbackSource(ProviderSearchRequestModel model);
        Task UpdateProviderSearchFilters(ProviderSearchViewModel viewModel);
        Task ClearProviderSearchFilters();
        Task SortProviderSearch(ProviderSearchSortRequestModel model);
        Task<ProviderConfirmViewModel> GetProviderConfirmViewModel(ProviderConfirmRequestModel model);
        Task<bool> ValidateProviderConfirmViewModel(ProviderConfirmViewModel viewModel, ModelStateDictionary modelState);
        Task CreateNewSurvey(ProviderConfirmViewModel viewModel);
    }
}