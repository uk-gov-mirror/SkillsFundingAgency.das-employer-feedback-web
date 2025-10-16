using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderSearchViewModel> GetTrainingProviderSearchViewModel(long accountId, string encodedAccountId, Guid userRef, string selectedProviderName, string selectedFeedbackStatus, int pageSize, int pageIndex, string sortColumn, string sortDirection);
        Task<ProviderSearchConfirmationViewModel> GetTrainingProviderConfirmationViewModel(long accountId, Guid userref, long providerId);
        bool CanSubmitFeedback(DateTime? dateTimeCompleted);
    }
}
