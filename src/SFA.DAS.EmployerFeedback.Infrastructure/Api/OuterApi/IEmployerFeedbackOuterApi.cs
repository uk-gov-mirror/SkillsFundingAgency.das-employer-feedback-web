using Microsoft.AspNetCore.Mvc;
using RestEase;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;


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
        Task<List<ProviderAttributeModel>> GetAllAttributes();

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

        Task<EmployerFeedbackResponse> GetEmployerFeedbackRecord(EmployerFeedbackRequest request);

        [Put("/employerfeedback/upsert")]
        Task<long> UpsertIntoFeedback(EmployerFeedbackRequest request);

        [Get("/employerfeedback/uniquesurveycode/{feedbackId}")]
        Task<Guid?> GetUniqueSurveyCodeFromFeedbackId([Path]long feedbackId);

        [Put("/employerfeedback/setcodeburntdate")]
        Task SetCodeBurntDate(Guid value);
        
        Task UpsertIntoProviders(Domain.Entities.Models.Provider[] providers);
        
        Task<GetProviderResponse> GetProvider(long providerId);

        Task<GetApprenticeshipsResponse> GetApprenticeships(long accountId);

        Task<IEnumerable<EmployerFeedbackViewModel>> GetEmployerFeedback();
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual(long ukprn);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary(long ukprn);
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary(string timePeriod);
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear(long ukPrn , int academicYear);
    }
}
