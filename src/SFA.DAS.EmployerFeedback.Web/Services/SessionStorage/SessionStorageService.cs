using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
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

        public async Task<SurveyModel> GetSurveyModel(string key)
        {
            return await Get<SurveyModel>(key);
        }

        public async Task<PagingState> GetPagingState(string key)
        {
            return await Get<PagingState>(key);
        }

        public async Task<FeedbackSource> GetFeedbackSource(string key)
        {
            return await Get<FeedbackSource>(key);
        }

        public async Task<string> GetString(string key)
        {
            return await _sessionCache.GetStringAsync(_environment + "_" + key);
        }

        public async Task<int> GetProviderCount(string key)
        {
            return await Get<int>(key);
        }

        public async Task SetPagingState(string key, PagingState pagingState)
        {
            await Set(key, pagingState);
        }

        public async Task SetSurveyModel(string key, SurveyModel surveyModel)
        {
            await Set(key, surveyModel);
        }

        public async Task Set(string key, object value)
        {
            await _sessionCache.SetStringAsync(_environment + "_" + key, JsonConvert.SerializeObject(value), new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(_slidingExpirationMinutes)
            });
        }

        public async Task Remove(string key)
        {
            await _sessionCache.RemoveAsync(_environment + "_" + key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await GetString(key) != null;
        }

        private async Task<T> Get<T>(string key)
        {
            var sessionObject = await GetString(key);
            return string.IsNullOrWhiteSpace(sessionObject) ? default(T) : JsonConvert.DeserializeObject<T>(sessionObject);
        }
    }
}
