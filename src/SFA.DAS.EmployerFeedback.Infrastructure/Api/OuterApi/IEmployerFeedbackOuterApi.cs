using RestEase;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IEmployerFeedbackOuterApi
    {
        [Get("/employerfeedback/{guid}")]
        Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode([Path] Guid guid);


        [Get("/accountusers/{userId}/accounts")]
        Task<UserAccountsDetails> GetUserAccounts([Path] string userId, [Query] string email);

        [Post("/employerfeebackresult")]
        Task<IEnumerable<EmployerFeedbackAndResult>> SubmitEmployerFeedback(EmployerFeedbackResult request);

        [Get("/attributes")]
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();

        [Get("/employerfeedback")]
        Task<ProviderFeedback> GetAllProviderFeedbackAndResult([Query("accountid")] long id, [Query("userref")] Guid userRef);

        [Get("/employerfeedback/iscodeburnt")]
        Task<bool> IsCodeBurnt(Guid emailCode);

        [Get("/employerfeedback/getcodeburntdate")]
        Task<DateTime?> GetCodeBurntDate(Guid uniqueCode);

        [Get("/ping")]
        Task Ping();

        [Get("/employerfeedback/healthcheck")]
        Task<string> HealthCheck();

        [Get("/employerfeedback/record")]
        Task<EmployerFeedbackResponse> GetEmployerFeedbackRecord(Guid userRef, long accountId, long ukprn);

        [Put("/employerfeedback/upsert")]
        Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn);

        [Get("/employerfeedback/uniquesurveycode/{feedbackId}")]
        Task<Guid?> GetUniqueSurveyCodeFromFeedbackId([Path]long feedbackId);

        [Put("/employerfeedback/setcodeburntdate")]
        Task SetCodeBurntDate(Guid value);

        [Post("/employerfeedback/providers")]
        Task UpsertIntoProviders(Domain.Entities.Models.Provider[] providers);

        [Get("/employerfeedback")]
        Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback();

        [Get("/employerfeedback/getfeedbackresultsummaryannual/{ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual([Path] long ukprn);

        [Get("/employerfeedback/getfeedbackresultsummary/{ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary([Path] long ukprn);


        [Get("/cohorts/accountIds")]
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds();

        [Get("/providers/{providerId}")]
        Task<GetProviderResponse> GetProvider([Path] long providerId);

        [Get("apprenticeships/")]
        Task<GetApprenticeshipsResponse> GetApprenticeships([Query] long accountId, [Query("pageNumber")] int pageNumber = 0, [Query("pageItemCount")] int pageItemCount = int.MaxValue);


        [Get("/employerfeedback/allstartsummary/{timePeriod}")]
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary([Path] string timePeriod);

        [Get("/employerfeedback/feedbackresultsummary")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear([Query("ukprn")] long ukPrn, [Query("academicYear")] string academicYear);
    }
}