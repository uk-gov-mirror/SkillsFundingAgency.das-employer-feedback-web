using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        public Task<SurveyModel> GetSurveyModel(string key);
        public Task<PagingState> GetPagingState(string key);
        
        public Task<FeedbackSource> GetFeedbackSource(string key);
        public Task<int> GetProviderCount(string key);

        public Task SetPagingState(string key, PagingState pagingState);

        public Task SetSurveyModel(string key, SurveyModel surveyModel);

        Task Set(string key, object value);
        Task Remove(string key);
        Task<bool> ExistsAsync(string key);
    }
}
