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
        public Task<SurveyModel> GetSurveyModel(Guid userId);
        public Task SetSurveyModel(Guid userId, SurveyModel surveyModel);
        public Task<PagingState> GetPagingState(Guid userId);
        public Task SetPagingState(Guid userId, PagingState pagingState);
        public Task<PagingState> UpdatePagingState(Guid userId, Action<PagingState> action);
        public Task<FeedbackSource> GetFeedbackSource(Guid userId);
        public Task SetFeedbackSource(Guid userId, FeedbackSource feedbackSource);
        public Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> GetProviders(Guid userId);
        public Task SetProviders(Guid userId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers);
    }
}
