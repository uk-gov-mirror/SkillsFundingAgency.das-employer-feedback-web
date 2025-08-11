using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    [ExcludeFromCodeCoverage]
    public class StandardsListItemViewModel
    {
        public string Id { get; set; }
        public int LarsCode { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public string Selected { get; set; }
    }
}