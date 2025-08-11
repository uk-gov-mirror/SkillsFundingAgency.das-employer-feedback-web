using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api
{
    public interface ICommitmentsOuterApi
    {
        [Get("/api/learners?{batch_size}&{batch_number}")]
        Task<LearnersResponse> GetLearners([Query] int batch_size, [Query] int batch_number);

        [Get("/ping")]
        Task Ping();
    }
}
