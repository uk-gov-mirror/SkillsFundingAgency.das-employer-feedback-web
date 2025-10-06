using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Identity.Client;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    //[ServiceFilter(typeof(EnsureFeedbackNotSubmittedRecentlyAttribute))]
    [ServiceFilter(typeof(EnsureSessionExists))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : Controller
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ReviewAnswersOrchestrator _orchestrator;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersController(
            ISessionStorageService sessionService
            , ReviewAnswersOrchestrator orchestrator
            , EmployerFeedbackWebConfiguration config
            )
        {
            _sessionService = sessionService;
            _orchestrator = orchestrator;
            _config = config;
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
            answers.Submitted = true;
            await _orchestrator.SubmitConfirmedEmployerFeedback(answers);
            await _sessionService.Set(idClaim.Value, answers);

            var accountId = HttpContext.GetRouteData().Values[RouteValueKeys.EncodedAccountId] as string;
            return RedirectToRoute(RouteNames.Confirmation_Get, new { encodedAccountId = accountId });
        }
    }
}
