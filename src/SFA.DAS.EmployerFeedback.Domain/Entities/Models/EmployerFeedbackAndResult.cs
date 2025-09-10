using System;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackAndResult : EmployerFeedbackResponse
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
