using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerRequest;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Extensions;
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
        private readonly IMediator _mediator;
        private readonly EmployerFeedbackWebConfiguration _config;

        public ReviewAnswersOrchestrator(IUserService userService,
            ILogger<ReviewAnswersOrchestrator> logger,
            ISessionStorageService sessionService,
            ITrainingProviderService trainingProviderService,
            IAccountsLinkService accountsLinkService,
            IMediator mediator,
            EmployerFeedbackWebConfiguration config)
            : base(logger, userService)
        {
            _sessionService = sessionService;
            _trainingProviderService = trainingProviderService;
            _accountsLinkService = accountsLinkService;
            _mediator = mediator;
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

        public async Task<bool> SubmitEmployerFeedback(ModelStateDictionary modelState)
        {
            var userId = GetUserId();
            var surveyModel = await _sessionService.GetSurveyModel(userId);

            var attributes = surveyModel.Attributes
                    .Where(s => s.Good || s.Bad)
                    .Select(p => new ProviderAttribute { AttributeId = p.AttributeId, AttributeValue = p.Score })
                    .ToList();

            var feedbackSubmitted = await _mediator.Send(new SubmitEmployerFeedbackCommand
            {
                Ukprn = surveyModel.Ukprn,
                AccountId = surveyModel.AccountId,
                Rating = surveyModel.Rating.ToString(),
                FeedbackSource = surveyModel.FeedbackSource,
                Attributes = attributes,
                UserRef = surveyModel.UserRef
            });

            if (!feedbackSubmitted)
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
