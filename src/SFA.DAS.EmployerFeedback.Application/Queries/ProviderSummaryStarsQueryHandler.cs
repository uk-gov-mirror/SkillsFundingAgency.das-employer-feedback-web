using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;

namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class ProviderSummaryStarsQueryHandler : IRequestHandler<ProviderSummaryStarsQuery, IEnumerable<EmployerFeedbackStarsSummary>>
    {
        private readonly ILogger<ProviderSummaryStarsQueryHandler> _logger;
        private readonly IEmployerFeedbackOuterApi _employerfeedbackOuterApi;

        public ProviderSummaryStarsQueryHandler(IEmployerFeedbackOuterApi employerFeedbackOuterApi,ILogger<ProviderSummaryStarsQueryHandler> logger)
        {
            _employerfeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerFeedbackStarsSummary>> Handle(ProviderSummaryStarsQuery request, CancellationToken token)
        {
            IEnumerable<ProviderStarsSummary> stars = await _employerfeedbackOuterApi.GetAllStarsSummary(request.TimePeriod);

            return stars?.Select(x => new EmployerFeedbackStarsSummary()
            {
                Ukprn = x.Ukprn,
                ReviewCount = x.ReviewCount,
                Stars = x.Stars,
            });
        }
    }
}
