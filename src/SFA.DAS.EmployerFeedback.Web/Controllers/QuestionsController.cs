using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.Controllers
{
    [Authorize(Policy = nameof(PolicyNames.NoneRole))]
    [ServiceFilter(typeof(EnsureSessionExists))]
    [Route(RoutePrefixPaths.FeedbackRoutePath)]
    public class QuestionsController : ControllerBase
    {
        private const string ReturnUrlKey = "ReturnUrl";
        private readonly ISessionStorageService _sessionService;

        public QuestionsController(ISessionStorageService sessionService, IUserService userService, ILogger<QuestionsController> logger) : base(userService, logger)
        {
            _sessionService = sessionService;
        }

        [HttpGet("question-one", Name = RouteNames.QuestionOne_Get)]
        public async Task<IActionResult> QuestionOne(string encodedAccountId, string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var cachedAnswers = await _sessionService.GetSurveyModel(GetUserId().Value.ToString());
            return View(cachedAnswers);
        }

        [HttpPost("question-one", Name = RouteNames.QuestionOne_Post)]
        public async Task<IActionResult> QuestionOne(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            var idClaim = GetUserId().Value.ToString();
            var sessionAnswer = await _sessionService.GetSurveyModel(idClaim);
            SetStengths(sessionAnswer, surveyModel.Attributes.Where(x => x.Good));
            await _sessionService.Set(idClaim, sessionAnswer);
            return await HandleRedirect(RouteNames.QuestionTwo_Get);
        }

        [HttpGet("question-two", Name = RouteNames.QuestionTwo_Get)]
        public async Task<IActionResult> QuestionTwo(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            var userId = GetUserId().Value.ToString();
            var sessionAnswers = await _sessionService.GetSurveyModel(userId);
            return View(sessionAnswers);
        }

        [HttpPost("question-two", Name = RouteNames.QuestionTwo_Post)]
        public async Task<IActionResult> QuestionTwo(SurveyModel surveyModel)
        {
            if (!IsProviderAttributesValid(surveyModel))
            {
                return View(surveyModel);
            }

            string userId = GetUserId().Value.ToString();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            SetWeaknesses(sessionAnswer, surveyModel.Attributes.Where(x => x.Bad));
            await _sessionService.Set(userId, sessionAnswer);
            return await HandleRedirect(RouteNames.QuestionThree_Get);
        }

        [HttpGet("question-three", Name = RouteNames.QuestionThree_Get)]
        public async Task<IActionResult> QuestionThree(string returnUrl = null)
        {
            TempData[ReturnUrlKey] = returnUrl;
            string userId = GetUserId().Value.ToString();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            return View(sessionAnswer);
        }

        [HttpPost("question-three", Name = RouteNames.QuestionThree_Post)]
        public async Task<IActionResult> QuestionThree(SurveyModel surveyModel)
        {
            if (!ModelState.IsValid)
            {
                return View(surveyModel);
            }

            string userId = GetUserId().Value.ToString();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            sessionAnswer.Rating = surveyModel.Rating;
            await _sessionService.Set(userId, sessionAnswer);
            return await HandleRedirect(RouteNames.ReviewAnswers_Get);
        }

        private async Task<IActionResult> HandleRedirect(string nextRoute)
        {
            var returnRoute = Convert.ToString(TempData[ReturnUrlKey]);
            string userId = GetUserId().Value.ToString();
            var sessionAnswer = await _sessionService.GetSurveyModel(userId);
            var accountId = HttpContext.GetRouteData().Values[RouteValueKeys.EncodedAccountId] as string;
            return await Task.Run(() => RedirectToRoute(string.IsNullOrEmpty(returnRoute) ? nextRoute : returnRoute, new { encodedAccountId = accountId }) as IActionResult);
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
