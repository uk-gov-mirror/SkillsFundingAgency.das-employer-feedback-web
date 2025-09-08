using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;


namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class ProviderSummaryStarsQuery : IRequest<IEnumerable<EmployerFeedbackStarsSummary>>
    {
        public string TimePeriod { get; set; }
    }
}
