using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [ServiceFilter(typeof(EnsureSessionExists))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        private readonly ISessionStorageService _sessionService;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly ReviewAnswersOrchestrator _orchestrator;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersController(ISessionStorageService sessionService, ReviewAnswersOrchestrator orchestrator
            , EmployerFeedbackWebConfiguration config, IEmployerFeedbackOuterApi employerFeedbackOuterApi)
        {
            _sessionService = sessionService;
            _orchestrator = orchestrator;
            _config = config;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
        }

        [HttpGet("review-answers", Name = RouteNames.ReviewAnswers_Get)]
        public async Task<IActionResult> Index()
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);
            var vm = await _sessionService.Get<SurveyModel>(idClaim.Value);
            vm.FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl;
            return View(vm);
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public async Task<IActionResult> Confirmation()
        {
            var idClaim = HttpContext.User.FindFirst(EmployerClaims.UserId);

            var answers = await _sessionService.Get<SurveyModel>(idClaim.Value);
            var trainingProviders = await _employerFeedbackOuterApi.GetTrainingProviderSearch(answers.AccountId, Guid.Parse(idClaim.Value));
            var providerFeedback = trainingProviders.Providers.Where(x => x.Ukprn == answers.Ukprn).First();
            var accountId = HttpContext.GetRouteData().Values[RouteValueKeys.EncodedAccountId] as string;

            if (providerFeedback.Feedback != null && (DateTime.UtcNow - providerFeedback.Feedback?.DateTimeCompleted).Value.TotalDays < _config.FeedbackWaitPeriodDays)
            {
                return RedirectToRoute(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = accountId });
            }

            answers.Submitted = true;
            await _orchestrator.SubmitConfirmedEmployerFeedback(answers);
            await _sessionService.Set(idClaim.Value, answers);
            return RedirectToRoute(RouteNames.Confirmation_Get, new { encodedAccountId = accountId });
        }
    }
}
