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

        [Get("/accountusers/{userId}/accounts?email={_email}")]
        Task<UserAccountsDetails> GetUserAccounts([Path] string userId, [Query] string email);

        [Post("/employerfeeback")]
        Task<IEnumerable<EmployerFeedbackAndResult>> SubmitEmployerFeedback(EmployerFeedbackResult request);

        [Get("/attributes")]
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();

        [Get("/employerfeedback/{guid}")]
        Task<EmployerSurveyInvite> GetEmployerInviteForUniqueCode([Path] Guid guid);

        [Get("/employerfeedback?accountid={id}&userref={UUID}")]
        Task<ProviderFeedback> GetAllProviderFeedbackAndResult([Query] long id, [Query] Guid userRef);

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
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual([Query] long ukprn);

        [Get("/employerfeedback/getfeedbackresultsummary/{ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn);


        [Get("/cohorts/accountIds")]
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds();

        [Get("/providers/{providerId}")]
        Task<GetProviderResponse> GetProvider([Path] long providerId);

        [Get("apprenticeships/?accountId={accountId}&pageNumber=0&pageItemCount={int.MaxValue}")]
        Task<GetApprenticeshipsResponse> GetApprenticeships([Query] long accountId);


        [Get("/employerfeedback/allstartsummary/{timePeriod}")]
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary([Path] string timePeriod);

        [Get("/employerfeedback/feedbackresultsummary?academicYear={academicYear}&ukprn={ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear([Query] long ukPrn, [Query] string academicYear);
    }
}