using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Confirmation;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;


namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [ServiceFilter(typeof(EnsureSessionExistsAttribute))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class ReviewAnswersController : ControllerBase
    {
        #region Routes
        public const string ReviewAnswersGet = nameof(ReviewAnswersGet);
        public const string ReviewAnswersPost = nameof(ReviewAnswersPost);
        public const string FeedbackConfirmationGet = nameof(FeedbackConfirmationGet);
        public const string FeedbackAlreadySubmittedGet = nameof(FeedbackAlreadySubmittedGet);
        #endregion

        private readonly ISessionStorageService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IAccountsLinkService _accountsLinkService;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersController(IUserService userService, 
            ILogger<ReviewAnswersController> logger, 
            ISessionStorageService sessionService, 
            ITrainingProviderService trainingProviderService,
            IAccountsLinkService accountsLinkService,
            EmployerFeedbackWebConfiguration config) 
            : base(userService, logger)
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _accountsLinkService = accountsLinkService;
            _config = config;
        }

        [HttpGet]
        [Route("review-answers", Name = ReviewAnswersGet)]
        public async Task<IActionResult> ReviewAnswers()
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());
            var viewModel = new ReviewAnswersViewModel
            {
                Survey = survey,
                FatSiteUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("review-answers", Name = ReviewAnswersPost)]
        public async Task<IActionResult> ReviewAnswersConfirmed()
        {
            var userId = GetUserId();
            var surveyModel = await _sessionService.GetSurveyModel(userId);

            var canSubmitFeedback = await _trainingProviderService.CanSubmitFeedback(surveyModel, userId);
            if (!canSubmitFeedback)
            {
                return RedirectToRoute(FeedbackAlreadySubmittedGet);
            }
            
            await _trainingProviderService.SubmitConfirmedEmployerFeedback(surveyModel);
            return RedirectToRoute(FeedbackConfirmationGet, new { encodedAccountId = surveyModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("feedback-confirmation", Name = FeedbackConfirmationGet)]
        public async Task<IActionResult> FeedbackConfirmation(string encodedAccountId)
        {
            var userId = GetUserId();

            var surveyModel = await _sessionService.GetSurveyModel(userId);
            await _sessionService.SetPagingState(userId, null);

            var viewModel = new ConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl,
                ComplaintSiteUrl = _config.ExternalLinks.ComplaintSiteUrl,
                ComplaintToProviderSiteUrl = _config.ExternalLinks.ComplaintToProviderSiteUrl,
                EncodedAccountId = encodedAccountId,
                EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(encodedAccountId)
            };

            return View(viewModel);
        }

        [HttpGet]
        [Route("feedback-submitted", Name = FeedbackAlreadySubmittedGet)]
        public IActionResult FeedbackAlreadySubmitted(string encodedAccountId)
        {
            var viewModel = new FeedbackAlreadySubmittedViewModel
            {
                EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(encodedAccountId)
            };

            return View(viewModel);
        }
    }
}
