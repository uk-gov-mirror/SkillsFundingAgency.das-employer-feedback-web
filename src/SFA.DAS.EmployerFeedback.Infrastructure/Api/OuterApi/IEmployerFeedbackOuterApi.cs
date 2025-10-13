using RestEase;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IEmployerFeedbackOuterApi
    {

        [Get("/attributes")]
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();

        [Post("/employerfeedbackresult")]
        Task<IEnumerable<EmployerFeedbackAndResult>> SubmitEmployerFeedback([Body] EmployerFeedbackResult request);

        [Get("/employerfeedback")]
        Task<GetProviderFeedback> GetTrainingProviderSearch([Query(Name = "accountid")] long id, [Query("userref")] Guid userRef);

        [Get("/ping")]
        Task Ping();

        [Get("/employerfeedback/healthcheck")]
        Task<string> HealthCheck();

        [Get("/accountusers/{userId}/accounts")]
        Task<UserAccountsDetails> GetUserAccounts([Path] string userId, [Query] string email);

    }
}