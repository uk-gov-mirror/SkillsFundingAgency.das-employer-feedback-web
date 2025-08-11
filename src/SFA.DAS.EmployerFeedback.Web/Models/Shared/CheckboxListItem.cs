using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.Models.Shared
{
    [ExcludeFromCodeCoverage]
    public class CheckboxListItem
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string Hint { get; set; }
    }
}
