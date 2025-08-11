using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests
{
    public class LearnersRequest
    {
        public int BatchNumber { get; set; }
        public int BatchSize { get; set; }

        public DateTime? SinceTime { get; set; }
    }
}
