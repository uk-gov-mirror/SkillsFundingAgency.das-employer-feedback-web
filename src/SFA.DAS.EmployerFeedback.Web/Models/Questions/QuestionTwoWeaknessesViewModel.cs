using System.Collections.Generic;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.Models.Questions
{
    public class QuestionTwoWeaknessesViewModel : AccountModel
    {
        public string ProviderName { get; set; }

        public List<ProviderAttributeModel> Attributes { get; set; } = new();
    }
}
