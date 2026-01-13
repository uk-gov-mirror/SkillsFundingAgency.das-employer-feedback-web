using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        Task<SurveyModel> GetSurveyModel(Guid userId);
        Task SetSurveyModel(Guid userId, SurveyModel surveyModel);
        Task<SurveyModel> UpdateSurveyModel(Guid userId, Action<SurveyModel> action);
        Task<PagingState> GetPagingState(Guid userId);
        Task SetPagingState(Guid userId, PagingState pagingState);
        Task<PagingState> UpdatePagingState(Guid userId, Action<PagingState> action);
        Task<FeedbackSource?> GetFeedbackSource(Guid userId);
        Task SetFeedbackSource(Guid userId, FeedbackSource feedbackSource);
        Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> GetProviders(Guid userId);
        Task SetProviders(Guid userId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers);
        Task ClearUserSession(Guid userId);
    }
}
