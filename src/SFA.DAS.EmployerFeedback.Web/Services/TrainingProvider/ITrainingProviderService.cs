using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(long accountId, string encodedAccountId, Guid userRef, string selectedProviderName, string selectedFeedbackStatus, int pageSize, int pageIndex, string sortColumn, string sortDirection);
        Task SubmitConfirmedEmployerFeedback(SurveyModel surveyModel);
        Task<bool> CanSubmitFeedback(SurveyModel surveyModel, Guid userId);
    }
}
