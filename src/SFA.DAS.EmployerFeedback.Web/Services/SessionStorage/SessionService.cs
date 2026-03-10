using MediatR;
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
        private readonly IMediator _mediator;
        public SessionService(ISessionStorageService sessionStorageService, IMediator mediator)
        {
            _sessionStorageService = sessionStorageService;
            _mediator = mediator;
        }
        public async Task<SurveyModel> GetSurveyModel(Guid userId)
        {
            var json = await _sessionStorageService.GetAsync(userId.ToString());
            var result = new SurveyModel();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<SurveyModel>(json);
            }

            return result;            
        }

        public async Task SetSurveyModel(Guid userId, SurveyModel surveyModel)
        {
            await _sessionStorageService.SetAsync(userId.ToString(), JsonSerializer.Serialize(surveyModel));
            
        }

        public async Task<SurveyModel> UpdateSurveyModel(Guid userId, Action<SurveyModel> action)
        {
            var surveyModel = await GetSurveyModel(userId) ?? new SurveyModel();
            action(surveyModel);
            await SetSurveyModel(userId, surveyModel);
            return surveyModel;
        }

        public async Task<PagingState> GetPagingState(Guid userId)
        {
            var json = await _sessionStorageService.GetAsync($"{userId}_PagingState");            

            var result = new PagingState();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<PagingState>(json);
            }

            return result;
        }

        public async Task SetPagingState(Guid userId, PagingState pagingState)
        {
            await _sessionStorageService.SetAsync($"{userId}_PagingState", JsonSerializer.Serialize(pagingState));            
        }

        public async Task<PagingState> UpdatePagingState(Guid userId, Action<PagingState> action)
        {
            var pagingState = await GetPagingState(userId) ?? new PagingState();
            action(pagingState);
            await SetPagingState(userId, pagingState);
            return pagingState;
        }

        public async Task<FeedbackSource?> GetFeedbackSource(Guid userId)
        {
            var json = await _sessionStorageService.GetAsync($"{userId}_FeedbackSource");
            var result = new FeedbackSource();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<FeedbackSource>(json);
            }

            return result;
        }

        public async Task SetFeedbackSource(Guid userId, FeedbackSource feedbackSource)
        {
            await _sessionStorageService.SetAsync($"{userId}_FeedbackSource", JsonSerializer.Serialize(feedbackSource));           
        }

        public async Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> GetProviders(Guid userId)
        {
            var json = await _sessionStorageService.GetAsync($"{userId}_Providers");
            var result = new List<ProviderSearchViewModel.EmployerTrainingProvider>();

            if (!string.IsNullOrWhiteSpace(json))
            {
                result = JsonSerializer.Deserialize<List<ProviderSearchViewModel.EmployerTrainingProvider>>(json);
            }

            return result;            
        }

        public async Task SetProviders(Guid userId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            await _sessionStorageService.SetAsync($"{userId}_Providers", JsonSerializer.Serialize(providers));

        }

        public async Task ClearUserSession(Guid userId)
        {
            await _sessionStorageService.ClearAsync(userId.ToString());
        }
    }
}
