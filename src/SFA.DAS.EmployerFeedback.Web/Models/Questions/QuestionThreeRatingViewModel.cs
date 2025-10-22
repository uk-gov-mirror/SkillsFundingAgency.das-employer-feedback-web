using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Questions
{
    public class QuestionThreeRatingViewModel : AccountModel
    {
        public ProviderRating? Rating { get; set; }
        public string ProviderName { get; set; }
    }
}
