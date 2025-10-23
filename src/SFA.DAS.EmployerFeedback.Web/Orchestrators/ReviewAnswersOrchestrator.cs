using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class ReviewAnswersOrchestrator : BaseOrchestrator, IReviewAnswersOrchestrator
    {
        private readonly ISessionStorageService _sessionService;
        private readonly ITrainingProviderService _trainingProviderService;
        private readonly IAccountsLinkService _accountsLinkService;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersOrchestrator(IUserService userService,
            ILogger<ReviewAnswersOrchestrator> logger,
            ISessionStorageService sessionService,
            ITrainingProviderService trainingProviderService,
            IAccountsLinkService accountsLinkService,
            EmployerFeedbackWebConfiguration config)
            : base(logger, userService)
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _accountsLinkService = accountsLinkService;
            _config = config;
        }

        public async Task<ReviewAnswersViewModel> GetReviewAnswersViewModel()
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());
            var viewModel = new ReviewAnswersViewModel
            {
                Survey = survey,
                FatSiteUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl
            };

            return viewModel;
        }

        public async Task<bool> CanSubmitFeedback()
        {
            var userId = GetUserId();
            var surveyModel = await _sessionService.GetSurveyModel(userId);
            return await _trainingProviderService.CanSubmitFeedback(surveyModel, userId);
        }

        public async Task<bool> SubmitFeedback(ModelStateDictionary modelState)
        {
            var userId = GetUserId();
            var surveyModel = await _sessionService.GetSurveyModel(userId);

            if (!await _trainingProviderService.SubmitConfirmedEmployerFeedback(surveyModel))
            {
                modelState.AddModelError(nameof(ReviewAnswersViewModel.Survey), "We couldn't submit your feedback right now. You can try again in a moment.");
                return false;
            }

            return true;
        }

        public async Task<FeedbackConfirmationViewModel> GetFeedbackConfirmationViewModel(AccountModel model)
        {
            var userId = GetUserId();
            var surveyModel = await _sessionService.GetSurveyModel(userId);
            await _sessionService.SetPagingState(userId, null);

            var viewModel = new FeedbackConfirmationViewModel
            {
                ProviderName = surveyModel.ProviderName,
                FeedbackRating = surveyModel.Rating.Value,
                FatUrl = _config.ExternalLinks.FindApprenticeshipTrainingSiteUrl,
                ComplaintSiteUrl = _config.ExternalLinks.ComplaintSiteUrl,
                ComplaintToProviderSiteUrl = _config.ExternalLinks.ComplaintToProviderSiteUrl,
                EncodedAccountId = model.EncodedAccountId,
                EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(model.EncodedAccountId)
            };

            return viewModel;
        }

        public FeedbackAlreadySubmittedViewModel GetFeedbackAlreadySubmittedViewModel(AccountModel model)
        {
            var viewModel = new FeedbackAlreadySubmittedViewModel
            {
                EmployerAccountsHomeUrl = _accountsLinkService.AccountsHome(model.EncodedAccountId)
            };

            return viewModel;
        }
    }
}
