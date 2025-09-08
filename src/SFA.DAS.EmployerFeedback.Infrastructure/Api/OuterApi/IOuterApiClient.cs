using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IOuterApiClient
    {
        Task<ApiResponse<TResponse>> Get<TResponse>(IGetApiRequest request);   
    }
}