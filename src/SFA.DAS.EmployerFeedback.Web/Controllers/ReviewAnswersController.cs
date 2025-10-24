using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
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

        private readonly IReviewAnswersOrchestrator _reviewAnswersOrchestrator;

        public ReviewAnswersController(IUserService userService, 
            ILogger<ReviewAnswersController> logger, 
            IReviewAnswersOrchestrator reviewAnswersOrchestrator) 
            : base(userService, logger)
        {
            _reviewAnswersOrchestrator = reviewAnswersOrchestrator;
        }

        [HttpGet]
        [Route("review-answers", Name = ReviewAnswersGet)]
        public async Task<IActionResult> ReviewAnswers()
        {
            return View(await _reviewAnswersOrchestrator.GetReviewAnswersViewModel());
        }

        [HttpPost]
        [Route("review-answers", Name = ReviewAnswersPost)]
        public async Task<IActionResult> ReviewAnswersConfirmed(ReviewAnswersViewModel viewModel)
        {
            if (!await _reviewAnswersOrchestrator.CanSubmitFeedback())
            {
                return RedirectToRoute(FeedbackAlreadySubmittedGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }
            
            if(!await _reviewAnswersOrchestrator.SubmitEmployerFeedback(ModelState))
            {
                return RedirectToRoute(ReviewAnswersGet, new { encodedAccountId = viewModel.EncodedAccountId });
            }

            return RedirectToRoute(FeedbackConfirmationGet, new { encodedAccountId = viewModel.EncodedAccountId });
        }

        [HttpGet]
        [Route("feedback-confirmation", Name = FeedbackConfirmationGet)]
        public async Task<IActionResult> FeedbackConfirmation(AccountModel model)
        {
            return View(await _reviewAnswersOrchestrator.GetFeedbackConfirmationViewModel(model));
        }

        [HttpGet]
        [Route("feedback-submitted", Name = FeedbackAlreadySubmittedGet)]
        public IActionResult FeedbackAlreadySubmitted(AccountModel model)
        {
            return View(_reviewAnswersOrchestrator.GetFeedbackAlreadySubmittedViewModel(model));
        }
    }
}
