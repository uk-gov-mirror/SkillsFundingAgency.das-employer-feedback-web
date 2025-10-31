using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Application.Queries.GetAllQuestionAttributes
{
    public class GetAllQuestionAttributesQueryHandler : IRequestHandler<GetAllQuestionAttributesQuery, IEnumerable<QuestionAttribute>>
    {
        private readonly IEmployerFeedbackOuterApi _outerApi;

        public GetAllQuestionAttributesQueryHandler(IEmployerFeedbackOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<IEnumerable<QuestionAttribute>> Handle(GetAllQuestionAttributesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _outerApi.GetAllQuestionAttributes();
            }
            catch (RestEase.ApiException ex)
            {
                throw new InvalidOperationException($"The question attributes cannot be retrieved", ex);
            }
        }
    }
}
