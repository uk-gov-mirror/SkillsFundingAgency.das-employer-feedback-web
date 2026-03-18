using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public interface IQuestionsOrchestrator
    {
        StartFeedbackViewModel GetStartFeedbackViewModel(AccountModel model);
        QuestionOneStrengthsViewModel GetQuestionOneStrengthsViewModel(QuestionRequestModel model);
        bool ValidateQuestionOneStrengthsViewModel(QuestionOneStrengthsViewModel viewModel, ModelStateDictionary modelState);
        void UpdateQuestionOneAnswers(QuestionOneStrengthsViewModel viewModel);
        QuestionTwoWeaknessesViewModel GetQuestionTwoWeaknessesViewModel(QuestionRequestModel model);
        bool ValidateQuestionTwoWeaknessesViewModel(QuestionTwoWeaknessesViewModel viewModel, ModelStateDictionary modelState);
        void UpdateQuestionTwoAnswers(QuestionTwoWeaknessesViewModel viewModel);
        QuestionThreeRatingViewModel GetQuestionThreeRatingViewModel(QuestionRequestModel model);
        bool ValidateQuestionThreeRatingViewModel(QuestionThreeRatingViewModel viewModel, ModelStateDictionary modelState);
        void UpdateQuestionThreeAnswers(QuestionThreeRatingViewModel viewModel);
    }
}