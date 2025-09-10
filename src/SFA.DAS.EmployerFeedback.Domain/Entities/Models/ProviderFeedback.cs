using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Domain.Entities.Models
{
    [ExcludeFromCodeCoverage]
    public class ProviderFeedback
    {
        public long Ukprn { get; set; }

        public string ProviderName { get; set; }

        public Feedback? Feedback { get; set; }

        public bool HasNewStart { get; set; }

        public bool HasActive { get; set; }

        public bool HasCompleted { get; set; }

    }
}
