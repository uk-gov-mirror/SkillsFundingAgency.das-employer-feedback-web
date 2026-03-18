using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
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

        private readonly IQuestionsOrchestrator _questionsOrchestrator;

        public QuestionsController(ISessionService sessionService, IUserService userService, 
            ILogger<QuestionsController> logger, IQuestionsOrchestrator questionsOrchestrator)
            : base(userService, logger)
        {
            _questionsOrchestrator = questionsOrchestrator;
        }

        [HttpGet]
        [Route("landing", Name = StartFeedbackGet)]
        public IActionResult StartFeedback(AccountModel model)
        {
            return View(_questionsOrchestrator.GetStartFeedbackViewModel(model));
        }

        [HttpGet]
        [Route("question-one", Name = QuestionOneGet)]
        public IActionResult QuestionOne(QuestionRequestModel model)
        {
            return View(_questionsOrchestrator.GetQuestionOneStrengthsViewModel(model));
        }

        [HttpPost]
        [Route("question-one", Name = QuestionOnePost)]
        public IActionResult QuestionOne(QuestionOneStrengthsViewModel viewModel)
        {
            if (!_questionsOrchestrator.ValidateQuestionOneStrengthsViewModel(viewModel, ModelState))
            {
                return RedirectToRoute(QuestionOneGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });
            }

            _questionsOrchestrator.UpdateQuestionOneAnswers(viewModel);

            if (viewModel.ReturnToReviewAnswers)
            {
                return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }

            return RedirectToRoute(QuestionTwoGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("question-two", Name = QuestionTwoGet)]
        public IActionResult QuestionTwo(QuestionRequestModel model)
        {
            return View(_questionsOrchestrator.GetQuestionTwoWeaknessesViewModel(model));
        }

        [HttpPost]
        [Route("question-two", Name = QuestionTwoPost)]
        public IActionResult QuestionTwo(QuestionTwoWeaknessesViewModel viewModel)
        {
            if (!_questionsOrchestrator.ValidateQuestionTwoWeaknessesViewModel(viewModel, ModelState))
            {
                return RedirectToRoute(QuestionTwoGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });
            }

            _questionsOrchestrator.UpdateQuestionTwoAnswers(viewModel);

            if (viewModel.ReturnToReviewAnswers)
            {
                return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }

            return RedirectToRoute(QuestionThreeGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("question-three", Name = QuestionThreeGet)]
        public IActionResult QuestionThree(QuestionRequestModel model)
        {
            return View(_questionsOrchestrator.GetQuestionThreeRatingViewModel(model));
        }

        [HttpPost]
        [Route("question-three", Name = QuestionThreePost)]
        public IActionResult QuestionThree(QuestionThreeRatingViewModel viewModel)
        {
            if (!_questionsOrchestrator.ValidateQuestionThreeRatingViewModel(viewModel, ModelState))
            {
                return RedirectToRoute(QuestionThreeGet, new { encodedAccountId = viewModel.EncodedAccountId, returnToReviewAnswers = viewModel.ReturnToReviewAnswers });
            }

            _questionsOrchestrator.UpdateQuestionThreeAnswers(viewModel);

            return RedirectToRoute(ReviewAnswersController.ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }
    }
}