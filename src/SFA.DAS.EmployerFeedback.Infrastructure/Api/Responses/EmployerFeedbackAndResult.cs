using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    public class EmployerFeedbackAndResult : EmployerFeedbackResponse
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
