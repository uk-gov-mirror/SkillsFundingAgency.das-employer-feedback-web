using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public interface IQuestionsOrchestrator
    {
        Task<StartFeedbackViewModel> GetStartFeedbackViewModel(AccountModel model);
        Task<QuestionOneStrengthsViewModel> GetQuestionOneStrengthsViewModel(QuestionRequestModel model);
        Task<bool> ValidateQuestionOneStrengthsViewModel(QuestionOneStrengthsViewModel viewModel, ModelStateDictionary modelState);
        Task UpdateQuestionOneAnswers(QuestionOneStrengthsViewModel viewModel);
        Task<QuestionTwoWeaknessesViewModel> GetQuestionTwoWeaknessesViewModel(QuestionRequestModel model);
        Task<bool> ValidateQuestionTwoWeaknessesViewModel(QuestionTwoWeaknessesViewModel viewModel, ModelStateDictionary modelState);
        Task UpdateQuestionTwoAnswers(QuestionTwoWeaknessesViewModel viewModel);
        Task<QuestionThreeRatingViewModel> GetQuestionThreeRatingViewModel(QuestionRequestModel model);
        Task<bool> ValidateQuestionThreeRatingViewModel(QuestionThreeRatingViewModel viewModel, ModelStateDictionary modelState);
        Task UpdateQuestionThreeAnswers(QuestionThreeRatingViewModel viewModel);
    }
}