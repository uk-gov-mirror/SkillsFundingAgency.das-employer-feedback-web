using MediatR;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerFeedback
{
    public class SubmitEmployerFeedbackCommand : IRequest<bool>
    {
        public long Ukprn { get; set; }
        public long AccountId { get; set; }
        public string? Rating { get; set; }
        public FeedbackSource FeedbackSource { get; set; }
        public List<ProviderAttribute>? Attributes { get; set; }
        public Guid UserRef { get; set; }
    }
}
