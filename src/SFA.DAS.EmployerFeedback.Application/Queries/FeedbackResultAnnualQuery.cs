using MediatR;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackResultAnnualQuery : IRequest<EmployerFeedbackAnnualResultDto>
    {
        public long Ukprn { get; set; }
    }
}
