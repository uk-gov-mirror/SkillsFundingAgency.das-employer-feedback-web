using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public class QuestionsOrchestrator : BaseOrchestrator, IQuestionsOrchestrator
    {
        private readonly ISessionService _sessionService;
        private readonly IValidator<QuestionOneStrengthsViewModel> _questionOneStrengthsViewModelValidator;
        private readonly IValidator<QuestionTwoWeaknessesViewModel> _questionTwoWeaknessesViewModelValidator;
        private readonly IValidator<QuestionThreeRatingViewModel> _questionThreeRatingViewModelValidator;

        public QuestionsOrchestrator(ISessionService sessionService, ILogger<QuestionsOrchestrator> logger,
            IUserService userService,
            IValidator<QuestionOneStrengthsViewModel> questionOneStrengthsViewModelValidator,
            IValidator<QuestionTwoWeaknessesViewModel> questionTwoWeaknessesViewModelValidator,
            IValidator<QuestionThreeRatingViewModel> questionThreeRatingViewModelValidator)
            : base(logger, userService)
        {
            _sessionService = sessionService;
            _questionOneStrengthsViewModelValidator = questionOneStrengthsViewModelValidator;
            _questionTwoWeaknessesViewModelValidator = questionTwoWeaknessesViewModelValidator;
            _questionThreeRatingViewModelValidator = questionThreeRatingViewModelValidator;
        }

        public StartFeedbackViewModel GetStartFeedbackViewModel(AccountModel model)
        {
            var survey = _sessionService.GetSurveyModel();
            var viewModel = new StartFeedbackViewModel
            {
                EncodedAccountId = model.EncodedAccountId,
                ProviderName = survey.ProviderName
            };

            return viewModel;
        }

        public QuestionOneStrengthsViewModel GetQuestionOneStrengthsViewModel(QuestionRequestModel model)
        {
            var survey = _sessionService.GetSurveyModel();

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

            return viewModel;
        }

        public bool ValidateQuestionOneStrengthsViewModel(QuestionOneStrengthsViewModel viewModel, ModelStateDictionary modelState)
        {
            return ValidateViewModel(_questionOneStrengthsViewModelValidator, viewModel, modelState);
        }

        public void  UpdateQuestionOneAnswers(QuestionOneStrengthsViewModel viewModel)
        {
            _sessionService.UpdateSurveyModel((SurveyModel survey) =>
            {
                foreach (var a in survey.Attributes)
                {
                    var match = viewModel.Attributes.Single(x => x.Name == a.Name);
                    a.Good = match.Good;
                }
            });
        }

        public QuestionTwoWeaknessesViewModel GetQuestionTwoWeaknessesViewModel(QuestionRequestModel model)
        {
            var survey = _sessionService.GetSurveyModel();

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

            return viewModel;
        }

        public bool ValidateQuestionTwoWeaknessesViewModel(QuestionTwoWeaknessesViewModel viewModel, ModelStateDictionary modelState)
        {
            return ValidateViewModel(_questionTwoWeaknessesViewModelValidator, viewModel, modelState);
        }

        public void UpdateQuestionTwoAnswers(QuestionTwoWeaknessesViewModel viewModel)
        {
            _sessionService.UpdateSurveyModel((SurveyModel survey) =>
            {
                foreach (var a in survey.Attributes)
                {
                    var match = viewModel.Attributes.Single(x => x.Name == a.Name);
                    a.Bad = match.Bad;
                }
            });
        }

        public QuestionThreeRatingViewModel GetQuestionThreeRatingViewModel(QuestionRequestModel model)
        {
            var survey = _sessionService.GetSurveyModel();

            var viewModel = new QuestionThreeRatingViewModel
            {
                EncodedAccountId = survey.EncodedAccountId,
                ProviderName = survey.ProviderName,
                Rating = survey.Rating,
                ReturnToReviewAnswers = model.ReturnToReviewAnswers
            };

            return viewModel;
        }

        public bool ValidateQuestionThreeRatingViewModel(QuestionThreeRatingViewModel viewModel, ModelStateDictionary modelState)
        {
            return ValidateViewModel(_questionThreeRatingViewModelValidator, viewModel, modelState);
        }

        public void UpdateQuestionThreeAnswers(QuestionThreeRatingViewModel viewModel)
        {
            _sessionService.UpdateSurveyModel((SurveyModel survey) =>
            {
                survey.Rating = viewModel.Rating;
            });
        }
    }
}
