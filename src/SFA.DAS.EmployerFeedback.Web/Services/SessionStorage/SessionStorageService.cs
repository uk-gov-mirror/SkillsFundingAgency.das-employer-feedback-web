using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public class SessionStorageService : ISessionStorageService
    {
        private readonly IDistributedCache _sessionCache;
        private readonly string _environment;
        private readonly int _slidingExpirationMinutes;

        public SessionStorageService(
            IDistributedCache sessionCache,
            EmployerFeedbackWebConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _slidingExpirationMinutes = configuration.SlidingExpirationMinutes;
            _sessionCache = sessionCache;
            _environment = environment.EnvironmentName;
        }

        public async Task<SurveyModel> GetSurveyModel(Guid userId)
        {
            return await Get<SurveyModel>(userId.ToString());
        }

        public async Task SetSurveyModel(Guid userId, SurveyModel surveyModel)
        {
            await Set(userId.ToString(), surveyModel);
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
            return await Get<PagingState>($"{userId}_PagingState");
        }
        public async Task SetPagingState(Guid userId, PagingState pagingState)
        {
            await Set($"{userId}_PagingState", pagingState);
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
            var value = await GetString($"{userId}_FeedbackSource");

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (Enum.TryParse<FeedbackSource>(value, true, out var result)
                && Enum.IsDefined(typeof(FeedbackSource), result))
            {
                return result;
            }

            return null;
        }

        public async Task SetFeedbackSource(Guid userId, FeedbackSource feedbackSource)
        {
            await Set($"{userId}_FeedbackSource", feedbackSource);
        }

        public async Task<List<ProviderSearchViewModel.EmployerTrainingProvider>> GetProviders(Guid userId)
        {
            return await Get<List<ProviderSearchViewModel.EmployerTrainingProvider>>($"{userId}_Providers");
        }

        public async Task SetProviders(Guid userId, List<ProviderSearchViewModel.EmployerTrainingProvider> providers)
        {
            await Set($"{userId}_Providers", providers);
        }

        private async Task<string> GetString(string key)
        {
            return await _sessionCache.GetStringAsync(_environment + "_" + key);
        }

        private async Task<T> Get<T>(string key)
        {
            var sessionObject = await GetString(key);
            return string.IsNullOrWhiteSpace(sessionObject) ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject);
        }

        private async Task Set(string key, object value)
        {
            await _sessionCache.SetStringAsync(_environment + "_" + key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(_slidingExpirationMinutes)
            });
        }

        public async Task ClearUserSession(Guid userId)
        {
            var keys = new[]
            {
                userId.ToString(),
                $"{userId}_PagingState",
                $"{userId}_FeedbackSource",
                $"{userId}_Providers"
            };

            foreach (var key in keys)
            {
                await _sessionCache.RemoveAsync(_environment + "_" + key);
            }
        }
    }
}
