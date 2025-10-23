using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Orchestrators
{
    public interface IReviewAnswersOrchestrator
    {
        Task<ReviewAnswersViewModel> GetReviewAnswersViewModel();
        Task<bool> CanSubmitFeedback();
        Task<bool> SubmitFeedback(ModelStateDictionary modelState);
        Task<FeedbackConfirmationViewModel> GetFeedbackConfirmationViewModel(AccountModel model);
        FeedbackAlreadySubmittedViewModel GetFeedbackAlreadySubmittedViewModel(AccountModel model);
    }
}