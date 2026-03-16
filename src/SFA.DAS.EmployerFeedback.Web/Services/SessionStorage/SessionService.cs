using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public class SessionService : ISessionService
    {
        private readonly ISessionStorageService _sessionStorageService;
        private const string SurveyModelKey = "SurveyModel";
        private const string PagingStateKey = "PagingState"; 
        private const string FeedbackSourceKey = "FeedbackSource";
        private const string ProvidersKey = "Providers";       
        
        public SessionService(ISessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;          
        }

        public  Task<SurveyModel> GetSurveyModel(Guid userId)
        {
            var json = _sessionStorageService.Get(SurveyModelKey);
            var result = new SurveyModel();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<SurveyModel>(json);
            }

            return Task.FromResult(result);            
        }

        public Task SetSurveyModel(Guid userId, SurveyModel surveyModel)
        {
            _sessionStorageService.Set(SurveyModelKey, JsonSerializer.Serialize(surveyModel));

            return Task.CompletedTask;            
        }

        public async Task<SurveyModel> UpdateSurveyModel(Guid userId, Action<SurveyModel> action)
        {
            var surveyModel = await GetSurveyModel(userId) ?? new SurveyModel();
            action(surveyModel);
            await SetSurveyModel(userId, surveyModel);
            return surveyModel;
        }

        public Task<PagingState> GetPagingState()
        {
            var json = _sessionStorageService.Get(PagingStateKey);            

            var result = new PagingState();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<PagingState>(json);
            }

            return Task.FromResult(result);
        }

        public Task SetPagingState(PagingState pagingState)
        {
             _sessionStorageService.Set(PagingStateKey, JsonSerializer.Serialize(pagingState)); 
            return Task.CompletedTask;
        }

        public async Task<PagingState> UpdatePagingState(Action<PagingState> action)
        {
            var pagingState = await GetPagingState() ?? new PagingState();
            action(pagingState);
            await SetPagingState(pagingState);
            return pagingState;
        }

        public async Task<FeedbackSource?> GetFeedbackSource(Guid userId)
        {
            var json = _sessionStorageService.Get(FeedbackSourceKey);           

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<FeedbackSource>(json);
            return Enum.IsDefined(typeof(FeedbackSource), result) ? result : null;
        }

        public Task SetFeedbackSource(Guid userId, FeedbackSource feedbackSource)
        {
            _sessionStorageService.Set(FeedbackSourceKey, JsonSerializer.Serialize(feedbackSource));  
            return Task.CompletedTask;
        }

        public  Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> GetProviders(Guid userId)
        {
            var json = _sessionStorageService.Get(ProvidersKey);
            var result = new List<ProviderSearchViewModel.EmployerTrainingProvider>();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<List<ProviderSearchViewModel.EmployerTrainingProvider>>(json);
            }

            return Task.FromResult(result);            
        }

        public  Task SetProviders(Guid userId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            _sessionStorageService.Set(ProvidersKey, JsonSerializer.Serialize(providers));
            return Task.CompletedTask;

        }

        public Task ClearUserSession(Guid userId)
        {
            _sessionStorageService.Clear(SurveyModelKey);
            _sessionStorageService.Clear(PagingStateKey);
            _sessionStorageService.Clear(FeedbackSourceKey);
            _sessionStorageService.Clear(ProvidersKey);
            return Task.CompletedTask;
        }
    }
}
