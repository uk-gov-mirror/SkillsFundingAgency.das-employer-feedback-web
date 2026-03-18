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
        void SetProviderSearchPageIndex(int pageIndex);
        void SetProviders(List<ProviderSearchViewModel.EmployerTrainingProvider> providers);
        void SetFeedbackSource(FeedbackSource feedbackSource);
        void UpdateProviderSearchFilters(ProviderSearchViewModel viewModel);
        void ClearProviderSearchFilters();
        void SortProviderSearch(SortColumn sortColumn, SortOrder sortOrder);
        ProviderConfirmViewModel GetProviderConfirmViewModel(ProviderConfirmRequestModel model);
        bool ValidateProviderConfirmViewModel(ProviderConfirmViewModel viewModel, ModelStateDictionary modelState);
        Task CreateNewSurvey(ProviderConfirmViewModel viewModel);
    }
}