using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Queries.GetTrainingProviderSearch
{
    public class GetTrainingProviderSearchQuery : IRequest<TrainingProviderSearchResponse>
    {
        public long AccountId { get; set; }
        public Guid UserRef { get; set; }
    }
}
