using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
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

        private const string ReturnUrlKey = "ReturnUrl";
        
        private readonly ISessionStorageService _sessionService;
        private readonly ILogger<QuestionsController> _logger;

        public QuestionsController(ISessionStorageService sessionService, IUserService userService, ILogger<QuestionsController> logger) 
            : base(userService, logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpGet]
        [Route("landing", Name = StartFeedbackGet)]
        public async Task<IActionResult> StartFeedback(AccountModel model)
        {
            try
            {
                var surveyModel = await _sessionService.GetSurveyModel(GetUserId());
                var viewModel = new StartFeedbackViewModel
                {
                    EncodedAccountId = model.EncodedAccountId,
                    ProviderName = surveyModel.ProviderName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StartFeedback");
                return RedirectToRoute(ErrorController.ErrorGet);
            }
        }

        [HttpGet("question-one", Name = QuestionOneGet)]
        public async Task<IActionResult> QuestionOne(string encodedAccountId, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var cachedAnswers = await _sessionService.GetSurveyModel(GetUserId());
            return View(cachedAnswers);
        }

        [HttpPost("question-one", Name = QuestionOnePost)]
        public async Task<IActionResult> QuestionOne(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            var userId = GetUserId();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            SetStengths(sessionAnswer, surveyModel.Attributes.Where(x => x.Good));
            await _sessionService.SetSurveyModel(userId, sessionAnswer);
            return await HandleRedirect(QuestionTwoGet);
        }

        [HttpGet("question-two", Name = QuestionTwoGet)]
        public async Task<IActionResult> QuestionTwo(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswers = await _sessionService.GetSurveyModel(GetUserId());
            return View(sessionAnswers);
        }

        [HttpPost("question-two", Name = QuestionTwoPost)]
        public async Task<IActionResult> QuestionTwo(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            var userId = GetUserId();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            SetWeaknesses(sessionAnswer, surveyModel.Attributes.Where(x => x.Bad));
            await _sessionService.SetSurveyModel(userId, sessionAnswer);
            return await HandleRedirect(QuestionThreeGet);
        }

        [HttpGet("question-three", Name = QuestionThreeGet)]
        public async Task<IActionResult> QuestionThree(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var sessionAnswer = await _sessionService.GetSurveyModel(GetUserId());
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = QuestionThreePost)]
        public async Task<IActionResult> QuestionThree(SurveyModel surveyModel)
        {
            if (!ModelState.IsValid)
            {
                return View(surveyModel);
            }

            var userId = GetUserId();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            sessionAnswer.Rating = surveyModel.Rating;
            await _sessionService.SetSurveyModel(userId, sessionAnswer);
            return await HandleRedirect(ReviewAnswersController.ReviewAnswersGet);
        }

        private async Task<IActionResult> HandleRedirect(string nextRoute)
        {
            var returnRoute = Convert.ToString(TempData[ReturnUrlKey]);
            var sessionAnswer = await _sessionService.GetSurveyModel(GetUserId());
            return await Task.Run(() => RedirectToRoute(string.IsNullOrEmpty(returnRoute) ? nextRoute : returnRoute, new { encodedAccountId = sessionAnswer.EncodedAccountId }) as IActionResult);
        }

        private bool IsProviderAttributesValid(SurveyModel surveyModel)
        {
            ModelState.TryGetValue(nameof(surveyModel.Attributes), out ModelStateEntry modelState);
            return modelState == null ? true : modelState.ValidationState == ModelValidationState.Valid;
        }

        private void SetStengths(SurveyModel sessionAnswer, IEnumerable<ProviderAttributeModel> currentAnswerAttributes)
        {
            foreach (var attr in sessionAnswer.Attributes)
            {
                var match = currentAnswerAttributes.SingleOrDefault(x => x.Name == attr.Name);
                attr.Good = match != null;
            }
        }

        private void SetWeaknesses(SurveyModel sessionAnswer, IEnumerable<ProviderAttributeModel> currentAnswerAttributes)
        {
            foreach (var attr in sessionAnswer.Attributes)
            {
                var match = currentAnswerAttributes.SingleOrDefault(x => x.Name == attr.Name);
                attr.Bad = match != null;
            }
        }
    }
}
