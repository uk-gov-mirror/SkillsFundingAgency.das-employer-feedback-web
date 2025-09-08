using System;
using System.Threading.Tasks;
using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    internal interface IEmployerFeedbackOuterApi
    {
        [Get("/employer-feedback/healthcheck")]
        Task<string> HealthCheck();


        [Post("/employerfeeback")]
        Task<> SubmitEmployerFeedback(EmployerFeedbackRequest request);

        [Get("/attributes")]
        Task<> GetAllAttributes();

        [Get("/employerfeedback?accountid={id}&userref={UUID}")]
        Task<> GetAllProviderFeedbackAndResult([Query] long id, [Query] Guid userRef);
    }
}
