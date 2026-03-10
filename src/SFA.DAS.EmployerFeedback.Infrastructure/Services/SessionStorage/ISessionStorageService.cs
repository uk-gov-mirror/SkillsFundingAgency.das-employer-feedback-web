using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        Task SetAsync(string key, string value);
        Task<string?> GetAsync(string key);
        Task ClearAsync(string key);
    }
}
