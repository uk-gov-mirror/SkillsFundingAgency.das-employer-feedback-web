
using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackResultQuery : IRequest<EmployerFeedbackResultDto>
    {
        public long Ukprn { get; set; }
    }
}
