using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Web.Models.Error
{
    [ExcludeFromCodeCoverage]
    public class ErrorViewModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; }
    }
}
