using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Application.Queries;

namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class ProviderSummaryStarsQueryHandler : IRequestHandler<ProviderSummaryStarsQuery, IEnumerable<EmployerFeedbackStarsSummary>>
    {
        private readonly ILogger<ProviderSummaryStarsQueryHandler> _logger;

        public ProviderSummaryStarsQueryHandler(ILogger<ProviderSummaryStarsQueryHandler> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<EmployerFeedbackStarsSummary>> Handle(ProviderSummaryStarsQuery request, CancellationToken token)
        {
            //FIXME - replace call with outer api call
            //IEnumerable<ProviderStarsSummary> stars = await _employerfeedbackRepository.GetAllStarsSummary(request.TimePeriod);

            //return stars?.Select(x => new EmployerFeedbackStarsSummary()
            //{
            //    Ukprn = x.Ukprn,
            //    ReviewCount = x.ReviewCount,
            //    Stars = x.Stars,
            //});
            throw new NotImplementedException();
        }
    }
}
