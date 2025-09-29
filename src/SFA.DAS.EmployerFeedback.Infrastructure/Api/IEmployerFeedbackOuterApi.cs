using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System.Numerics;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Domain.Interfaces
{
    public interface IEmployerFeedbackOuterApi
    {

        [Get("/accountusers/{userId}/accounts")]
        Task<UserAccountsDetails> GetUserAccounts([Path] string userId, [Query] string email);

        [Get("/commitments/{providerId}")]
        Task<Provider> GetProviders([Path] long providerId);

        [Get("/ping")]
        Task Ping();
    }
}
