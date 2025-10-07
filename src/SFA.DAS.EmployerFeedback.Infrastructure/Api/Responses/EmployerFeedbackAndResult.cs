using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    [ExcludeFromCodeCoverage]
    public class EmployerFeedbackAndResult : EmployerFeedbackResponse
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
