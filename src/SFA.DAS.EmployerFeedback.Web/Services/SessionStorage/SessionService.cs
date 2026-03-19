using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
using System.Collections.Generic;
using System.Text.Json;

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

        public  SurveyModel GetSurveyModel()
        {
            var json = _sessionStorageService.Get(SurveyModelKey);
            var result = new SurveyModel();

            if (!string.IsNullOrWhiteSpace(json))
           {
                result = JsonSerializer.Deserialize<SurveyModel>(json);
           }

            return result;            
        }

        public void SetSurveyModel(SurveyModel surveyModel)
        {
            _sessionStorageService.Set(SurveyModelKey, JsonSerializer.Serialize(surveyModel));                        
        }

        public SurveyModel UpdateSurveyModel(Action<SurveyModel> action)
        {
            var surveyModel = GetSurveyModel() ?? new SurveyModel();
            action(surveyModel);
            SetSurveyModel(surveyModel);
            return surveyModel;
        }

        public PagingState GetPagingState()
        {
            var json = _sessionStorageService.Get(PagingStateKey);            

            var result = new PagingState();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<PagingState>(json);
            }

            return result;
        }

        public void SetPagingState(PagingState pagingState)
        {
             _sessionStorageService.Set(PagingStateKey, JsonSerializer.Serialize(pagingState));             
        }

        public PagingState UpdatePagingState(Action<PagingState> action)
        {
            var pagingState = GetPagingState() ?? new PagingState();
            action(pagingState);
            SetPagingState(pagingState);
            return pagingState;
        }

        public FeedbackSource? GetFeedbackSource()
        {
            var json = _sessionStorageService.Get(FeedbackSourceKey);           

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var result = JsonSerializer.Deserialize<FeedbackSource>(json);
            return Enum.IsDefined(typeof(FeedbackSource), result) ? result : null;
        }

        public void SetFeedbackSource(FeedbackSource feedbackSource)
        {
            _sessionStorageService.Set(FeedbackSourceKey, JsonSerializer.Serialize(feedbackSource));              
        }

        public  List<ProviderSearchViewModel.EmployerTrainingProvider> GetProviders()
        {
            var json = _sessionStorageService.Get(ProvidersKey);
            var result = new List<ProviderSearchViewModel.EmployerTrainingProvider>();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<List<ProviderSearchViewModel.EmployerTrainingProvider>>(json)
                         ?? new List<ProviderSearchViewModel.EmployerTrainingProvider>();
            }

            return result;            
        }

        public  void SetProviders(List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            _sessionStorageService.Set(ProvidersKey, JsonSerializer.Serialize(providers));            
        }

        public void ClearUserSession()
        {
            _sessionStorageService.Clear(SurveyModelKey);
            _sessionStorageService.Clear(PagingStateKey);
            _sessionStorageService.Clear(FeedbackSourceKey);
            _sessionStorageService.Clear(ProvidersKey);            
        }        
    }
}
