using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IEmployerFeedbackOuterApi
    {
        [Get("/attributes")]
        Task<IEnumerable<QuestionAttribute>> GetAllQuestionAttributes();

        [Post("/employerfeedbackresult")]
        Task<IEnumerable<SubmitEmployerFeedbackResponse>> SubmitEmployerFeedback([Body] SubmitEmployerFeedbackRequest request);

        [Get("/employerfeedback")]
        Task<TrainingProviderSearchResponse> GetTrainingProviderSearch([Query(Name = "accountid")] long id, [Query("userref")] Guid userRef);

        [Get("/ping")]
        Task Ping();

        [Get("/accountusers/{userId}/accounts")]
        Task<UserAccountsDetailsResponse> GetUserAccounts([Path] string userId, [Query] string email);
    }
}