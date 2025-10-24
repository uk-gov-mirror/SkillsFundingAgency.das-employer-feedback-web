using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{
    public class SubmitEmployerFeedbackResponse : EmployerFeedbackResponse
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
