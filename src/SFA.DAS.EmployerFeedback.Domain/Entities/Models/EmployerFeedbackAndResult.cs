using System;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackAndResult : EmployerFeedback
    {
        public DateTime? DateTimeCompleted { get; set; }
    }
}
