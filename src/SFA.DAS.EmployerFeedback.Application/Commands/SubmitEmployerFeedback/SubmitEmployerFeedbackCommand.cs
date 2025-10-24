using MediatR;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Domain.Types;

namespace SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerRequest
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
