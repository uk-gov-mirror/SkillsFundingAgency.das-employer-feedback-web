using System.Collections.Generic;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Questions
{
    public class QuestionOneStrengthsViewModel : AccountModel
    {
        public string ProviderName { get; set; }

        public List<ProviderAttributeModel> Attributes { get; set; } = new();

        public bool ReturnToReviewAnswers { get; set;}
    }
}
