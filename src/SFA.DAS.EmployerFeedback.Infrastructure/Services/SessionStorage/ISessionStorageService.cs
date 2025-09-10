using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        Task<T> Get<T>(string key);
        Task Set(string key, object value);
        Task Remove(string key);
        Task<bool> ExistsAsync(string key);
    }
}
