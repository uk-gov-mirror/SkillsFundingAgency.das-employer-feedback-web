using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [ServiceFilter(typeof(EnsureSessionExists))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : ControllerBase
    {
        private readonly ISessionStorageService _sessionService;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly ReviewAnswersOrchestrator _orchestrator;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersController(ISessionStorageService sessionService, ReviewAnswersOrchestrator orchestrator
            , EmployerFeedbackWebConfiguration config, IEmployerFeedbackOuterApi employerFeedbackOuterApi, IUserService userService, ILogger<ReviewAnswersController> logger, ITrainingProviderService trainingProviderService) : base(userService, logger)
        {
            _sessionService = sessionService;
            _orchestrator = orchestrator;
            _config = config;
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _trainingProviderService = trainingProviderService;
        }

        [HttpGet("review-answers", Name = RouteNames.ReviewAnswers_Get)]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId().Value.ToString();
            var vm = await _sessionService.GetSurveyModel(userId);
            vm.FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl;
            return View(vm);
        }

        [HttpPost("review-answers", Name = RouteNames.ReviewAnswers_Post)]
        public async Task<IActionResult> Confirmation()
        {
            var userId = GetUserId().Value;
            var answers = await _sessionService.GetSurveyModel(userId.ToString());
            var trainingProviders = await _employerFeedbackOuterApi.GetTrainingProviderSearch(answers.AccountId, userId);
            var providerFeedback = trainingProviders.Providers.Where(x => x.Ukprn == answers.Ukprn).First();

            if (! _trainingProviderService.CanSubmitFeedback(providerFeedback.Feedback?.DateTimeCompleted))
            {
                return RedirectToRoute(RouteNames.FeedbackAlreadySubmitted);
            }
            
            await _orchestrator.SubmitConfirmedEmployerFeedback(answers);
            return RedirectToRoute(RouteNames.Confirmation_Get, new { encodedAccountId = answers.EncodedAccountId });
        }
    }
}
