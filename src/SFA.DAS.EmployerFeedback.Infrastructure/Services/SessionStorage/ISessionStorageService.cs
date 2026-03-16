
namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage
{
    public interface ISessionStorageService
    {
        void Set(string key, string value);
        string? Get(string key);
        void Clear(string key);
    }
}
