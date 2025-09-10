using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    public class EmployerFeedbackViewModel
    {
        public Guid Id { get; set; }
        public long FeedbackId { get; set; }
        public long Ukprn { get; set; }
        public DateTime DateTimeCompleted { get; set; }
        public string ProviderRating { get; set; }
        public string AttributeName { get; set; }
        public int AttributeValue { get; set; }
    }
}
