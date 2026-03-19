using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Web.Services.SessionStorage
{
    public interface ISessionService
    {
        SurveyModel? GetSurveyModel();
        void SetSurveyModel(SurveyModel surveyModel);
        SurveyModel UpdateSurveyModel(Action<SurveyModel> action);
        PagingState GetPagingState();
        void SetPagingState(PagingState pagingState);
        PagingState UpdatePagingState(Action<PagingState> action);
        FeedbackSource? GetFeedbackSource();
        void SetFeedbackSource(FeedbackSource feedbackSource);
        List<ProviderSearchViewModel.EmployerTrainingProvider> GetProviders();
        void SetProviders(List<ProviderSearchViewModel.EmployerTrainingProvider> providers);
        void ClearUserSession();
    }
}
