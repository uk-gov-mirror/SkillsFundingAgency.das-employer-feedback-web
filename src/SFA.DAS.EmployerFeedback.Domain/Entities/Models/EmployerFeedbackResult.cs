using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackResult
    {
        public Guid Id { get; set; }
        public long FeedbackId { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
        public ICollection<ProviderAttribute> ProviderAttributes { get; set; }
    }
}
