using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public interface IProviderOrchestrator
    {
        Task<ProviderSearchViewModel> GetProviderSearchViewModel(ProviderSearchRequestModel model);
        Task SetProviderSearchPageIndex(int pageIndex);
        Task SetProviders(List<ProviderSearchViewModel.EmployerTrainingProvider> providers);
        Task SetFeedbackSource(FeedbackSource feedbackSource);
        Task UpdateProviderSearchFilters(ProviderSearchViewModel viewModel);
        Task ClearProviderSearchFilters();
        Task SortProviderSearch(SortColumn sortColumn, SortOrder sortOrder);
        Task<ProviderConfirmViewModel> GetProviderConfirmViewModel(ProviderConfirmRequestModel model);
        Task<bool> ValidateProviderConfirmViewModel(ProviderConfirmViewModel viewModel, ModelStateDictionary modelState);
        Task CreateNewSurvey(ProviderConfirmViewModel viewModel);
    }
}