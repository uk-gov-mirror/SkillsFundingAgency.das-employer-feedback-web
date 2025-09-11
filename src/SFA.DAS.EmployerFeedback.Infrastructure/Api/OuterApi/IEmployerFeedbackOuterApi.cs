using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestEase;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi
{
    public interface IEmployerFeedbackOuterApi
    {

        [Get("/accountusers/{userId}/accounts")]
        Task<UserAccountsDetails> GetUserAccounts([Path] string userId, [Query] string email);

        [Get("/employer-feedback/healthcheck")]
        Task<string> HealthCheck();


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

        [Get("/employerfeedback/record")]

        Task<EmployerFeedbackResponse> GetEmployerFeedbackRecord(Guid userRef, long accountId, long ukprn);

        [Put("/employerfeedback/upsert")]
        Task<long> UpsertIntoFeedback(Guid userRef, long accountId, long ukprn);

        [Get("/employerfeedback/uniquesurveycode/{feedbackId}")]
        Task<Guid?> GetUniqueSurveyCodeFromFeedbackId([Path]long feedbackId);

        [Put("/employerfeedback/setcodeburntdate")]
        Task SetCodeBurntDate(Guid value);
        
        Task UpsertIntoProviders(Domain.Entities.Models.Provider[] providers);

        [Get("/cohorts/accountIds")]
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds();


        [Get("/providers/{providerId}")]
        Task<GetProviderResponse> GetProvider([Path]long providerId);

        [Get("apprenticeships/?accountId={accountId}&pageNumber=0&pageItemCount={int.MaxValue}")]

        Task<GetApprenticeshipsResponse> GetApprenticeships([Query]long accountId);

        Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback();
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual(long ukprn);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn);
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary(string timePeriod);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear(long ukPrn , string academicYear);
    }
}