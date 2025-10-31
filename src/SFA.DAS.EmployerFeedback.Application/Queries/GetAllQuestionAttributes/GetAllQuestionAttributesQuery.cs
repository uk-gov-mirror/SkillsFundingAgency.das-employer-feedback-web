using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Application.Queries.GetAllQuestionAttributes
{
    public class GetAllQuestionAttributesQuery : IRequest<IEnumerable<QuestionAttribute>>
    {
    }
}
