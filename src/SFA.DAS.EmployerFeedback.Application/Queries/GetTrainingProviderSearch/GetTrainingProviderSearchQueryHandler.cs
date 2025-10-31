using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Queries.GetTrainingProviderSearch
{
    public class GetTrainingProviderSearchQueryHandler : IRequestHandler<GetTrainingProviderSearchQuery, TrainingProviderSearchResponse>
    {
        private readonly IEmployerFeedbackOuterApi _outerApi;

        public GetTrainingProviderSearchQueryHandler(IEmployerFeedbackOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<TrainingProviderSearchResponse> Handle(GetTrainingProviderSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _outerApi.GetTrainingProviderSearch(request.AccountId, request.UserRef);
            }
            catch (RestEase.ApiException ex)
            {
                throw new InvalidOperationException($"The training provider search cannot be retrieved", ex);
            }
        }
    }
}
