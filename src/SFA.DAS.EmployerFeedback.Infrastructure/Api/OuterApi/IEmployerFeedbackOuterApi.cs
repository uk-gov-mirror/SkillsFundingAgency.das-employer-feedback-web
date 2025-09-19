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

        [Get("/attributes")]
        Task<IEnumerable<FeedbackQuestionAttribute>> GetAllAttributes();

        [Post("/employerfeebackresult")]
        Task<IEnumerable<EmployerFeedbackAndResult>> SubmitEmployerFeedback(EmployerFeedbackResult request);

        [Get("/employerfeedback?accountid={id}&userref={UUID}")]
        Task<ProviderFeedback> GetTrainingProviderSearch([Query("accountid")] long id, [Query("userref")] Guid userRef);

        [Get("/ping")]
        Task Ping();

        [Get("/employerfeedback/healthcheck")]
        Task<string> HealthCheck();

        [Get("/employerfeedback/getfeedbackresultsummaryannual/{ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryAnnual([Path] long ukprn);

        [Get("/employerfeedback/getfeedbackresultsummary/{ukprn}")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummary([Path] long ukprn);


        [Get("/cohorts/accountIds")]
        Task<GetAllCohortAccountIdsResponse> GetAllCohortAccountIds();

        [Get("/employerfeedback/allstartsummary/{timePeriod}")]
        Task<IEnumerable<ProviderStarsSummary>> GetAllStarsSummary([Path] string timePeriod);

        [Get("/employerfeedback/feedbackresultsummary")]
        Task<IEnumerable<EmployerFeedbackResultSummary>> GetFeedbackResultSummaryForAcademicYear([Query("ukprn")] long ukPrn, [Query("academicYear")] string academicYear);
    }
}