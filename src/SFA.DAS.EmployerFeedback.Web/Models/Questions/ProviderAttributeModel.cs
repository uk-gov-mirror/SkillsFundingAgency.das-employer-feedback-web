using System;

namespace SFA.DAS.EmployerFeedback.Web.Models.Questions
{
    [Serializable]
    public class ProviderAttributeModel
    {
        public string Name { get; set; }
        public bool Good { get; set; }
        public bool Bad { get; set; }
        public int Score => Good ? 1 : Bad ? -1 : 0;
    }
}
