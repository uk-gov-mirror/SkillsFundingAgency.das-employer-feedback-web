using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;

namespace SFA.DAS.EmployerFeedback.Application.Commands
{

    public class FeedbackResultAnnualQueryHandler : IRequestHandler<FeedbackResultAnnualQuery, EmployerFeedbackAnnualResultDto>
    {
        private readonly ILogger<FeedbackResultAnnualQueryHandler> _logger;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        public FeedbackResultAnnualQueryHandler(IEmployerFeedbackOuterApi employerFeedbackOuterApi, ILogger<FeedbackResultAnnualQueryHandler> logger)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
        }
        public async Task<EmployerFeedbackAnnualResultDto> Handle(FeedbackResultAnnualQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerFeedbackOuterApi.GetFeedbackResultSummaryAnnual(request.Ukprn);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerFeedbackAnnualResultDto()
                {
                    AnnualEmployerFeedbackDetails = Enumerable.Empty<EmployerFeedbackStarsAnnualSummaryDto>()
                };
            }

            var grouped = feedback
                .Where(f => f.AttributeName != null)
                .GroupBy(
                    x => new { x.Ukprn, x.TimePeriod, x.Stars, x.ReviewCount },
                    (key, group) => new EmployerFeedbackStarsAnnualSummaryDto
                    {
                        Ukprn = key.Ukprn,
                        TimePeriod = key.TimePeriod,
                        Stars = key.Stars,
                        ReviewCount = key.ReviewCount,
                        ProviderAttribute = group
                            .Where(g => g.AttributeName != null)
                            .Select(g => new ProviderAttributeAnnualSummaryItemDto
                            {
                                Name = g.AttributeName,
                                Strength = g.Strength,
                                Weakness = g.Weakness
                            })
                            .ToList()
                    })
                .ToList();

            return new EmployerFeedbackAnnualResultDto
            {
                AnnualEmployerFeedbackDetails = grouped
            };
        }
    }
}
