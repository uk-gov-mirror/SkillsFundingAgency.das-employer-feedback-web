using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackResultAnnualQuery : IRequest<EmployerFeedbackAnnualResultDto>
    {
        public long Ukprn { get; set; }
    }
}
