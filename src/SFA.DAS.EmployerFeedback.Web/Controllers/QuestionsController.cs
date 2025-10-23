using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [ServiceFilter(typeof(EnsureSessionExistsAttribute))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class QuestionsController : ControllerBase
    {
        #region Routes
        public const string StartFeedbackGet = nameof(StartFeedbackGet);
        public const string QuestionOneGet = nameof(QuestionOneGet);
        public const string QuestionOnePost = nameof(QuestionOnePost);
        public const string QuestionTwoGet = nameof(QuestionTwoGet);
        public const string QuestionTwoPost = nameof(QuestionTwoPost);
        public const string QuestionThreeGet = nameof(QuestionThreeGet);
        public const string QuestionThreePost = nameof(QuestionThreePost);
        #endregion

        private readonly ISessionStorageService _sessionService;
        private readonly IValidator<QuestionOneStrengthsViewModel> _questionOneStrengthsViewModelValidator;
        private readonly IValidator<QuestionTwoWeaknessesViewModel> _questionTwoWeaknessesViewModelValidator;
        private readonly IValidator<QuestionThreeRatingViewModel> _questionThreeRatingViewModelValidator;

        public QuestionsController(ISessionStorageService sessionService, IUserService userService, ILogger<QuestionsController> logger,
            IValidator<QuestionOneStrengthsViewModel> questionOneStrengthsViewModelValidator,
            IValidator<QuestionTwoWeaknessesViewModel> questionTwoWeaknessesViewModelValidator,
            IValidator<QuestionThreeRatingViewModel> questionThreeRatingViewModelValidator)
            : base(userService, logger)
        {
            _sessionService = sessionService;
            _questionOneStrengthsViewModelValidator = questionOneStrengthsViewModelValidator;
            _questionTwoWeaknessesViewModelValidator = questionTwoWeaknessesViewModelValidator;
            _questionThreeRatingViewModelValidator = questionThreeRatingViewModelValidator;
        }

        [HttpGet]
        [Route("landing", Name = StartFeedbackGet)]
        public async Task<IActionResult> StartFeedback(AccountModel model)
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());
            var viewModel = new StartFeedbackViewModel
            {
                EncodedAccountId = model.EncodedAccountId,
                ProviderName = survey.ProviderName
            };
            return View(viewModel);
        }

        [HttpGet("question-one", Name = QuestionOneGet)]
        public async Task<IActionResult> QuestionOne(QuestionRequestModel model)
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());

            var viewModel = new QuestionOneStrengthsViewModel
            {
                EncodedAccountId = survey.EncodedAccountId,
                ProviderName = survey.ProviderName,
                Attributes = survey.Attributes.Select(a => new ProviderAttributeModel
                {
                    Name = a.Name,
                    Good = a.Good,
                    Bad = a.Bad
                }).ToList(),
                ReturnToReviewAnswers = model.ReturnToReviewAnswers
            };

            return View(viewModel);
        }

        [HttpPost("question-one", Name = QuestionOnePost)]
        public async Task<IActionResult> QuestionOne(QuestionOneStrengthsViewModel viewModel)
        {
            if (!await ViewModelIsValid(_questionOneStrengthsViewModelValidator, viewModel, ModelState))
                return RedirectToRoute(QuestionOneGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });

            var userId = GetUserId();
            await _sessionService.UpdateSurveyModel(userId, (SurveyModel survey) => 
            {
                foreach (var a in survey.Attributes)
                {
                    var match = viewModel.Attributes.Single(x => x.Name == a.Name);
                    a.Good = match.Good;
                }
            });

            if (viewModel.ReturnToReviewAnswers)
                return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });

            return RedirectToRoute(QuestionTwoGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }

        [HttpGet("question-two", Name = QuestionTwoGet)]
        public async Task<IActionResult> QuestionTwo(QuestionRequestModel model)
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());

            var viewModel = new QuestionTwoWeaknessesViewModel
            {
                EncodedAccountId = survey.EncodedAccountId,
                ProviderName = survey.ProviderName,
                Attributes = survey.Attributes.Select(a => new ProviderAttributeModel
                {
                    Name = a.Name,
                    Good = a.Good,
                    Bad = a.Bad
                }).ToList(),
                ReturnToReviewAnswers = model.ReturnToReviewAnswers
            };

            return View(viewModel);
        }

        [HttpPost("question-two", Name = QuestionTwoPost)]
        public async Task<IActionResult> QuestionTwo(QuestionTwoWeaknessesViewModel viewModel)
        {
            if (!await ViewModelIsValid(_questionTwoWeaknessesViewModelValidator, viewModel, ModelState))
                return RedirectToRoute(QuestionTwoGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });

            var userId = GetUserId();
            await _sessionService.UpdateSurveyModel(userId, (SurveyModel survey) =>
            {
                foreach (var a in survey.Attributes)
                {
                    var match = viewModel.Attributes.Single(x => x.Name == a.Name);
                    a.Bad = match.Bad;
                }
            });

            if (viewModel.ReturnToReviewAnswers)
                return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });

            return RedirectToRoute(QuestionThreeGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }

        [HttpGet("question-three", Name = QuestionThreeGet)]
        public async Task<IActionResult> QuestionThree(QuestionRequestModel model)
        {
            var survey = await _sessionService.GetSurveyModel(GetUserId());

            var viewModel = new QuestionThreeRatingViewModel
            {
                EncodedAccountId = survey.EncodedAccountId,
                ProviderName = survey.ProviderName,
                Rating = survey.Rating,
                ReturnToReviewAnswers = model.ReturnToReviewAnswers
            };

            return View(viewModel);
        }

        [HttpPost("question-three", Name = QuestionThreePost)]
        public async Task<IActionResult> QuestionThree(QuestionThreeRatingViewModel viewModel)
        {
            if (!await ViewModelIsValid(_questionThreeRatingViewModelValidator, viewModel, ModelState))
                return RedirectToRoute(QuestionThreeGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });
            
            var userId = GetUserId();
            await _sessionService.UpdateSurveyModel(userId, (SurveyModel survey) =>
            {
                survey.Rating = viewModel.Rating;
            });

            return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }
    }
}