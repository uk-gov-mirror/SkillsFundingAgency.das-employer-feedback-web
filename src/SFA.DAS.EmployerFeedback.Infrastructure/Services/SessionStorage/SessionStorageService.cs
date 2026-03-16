using Microsoft.AspNetCore.Http;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage
{
    public class SessionStorageService : ISessionStorageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
              
        public SessionStorageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;            
        }

        public void Set(string key, string value)
        {
            Session?.SetString(key, value);
           
        }

        public string? Get(string key)
        {
            return Session?.GetString(key);
        }

        public void Clear(string key)
        {
            Session?.Remove(key);
           
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;
    }
}
