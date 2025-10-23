using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers
{
    public class ReviewAnswersViewModel : AccountModel
    {
        public SurveyModel Survey { get; set; }
        public string FatSiteUrl { get; set; }
    }
}
