using MediatR;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackQuery : IRequest<IEnumerable<EmployerFeedbackDto>>
    {
    }
}
