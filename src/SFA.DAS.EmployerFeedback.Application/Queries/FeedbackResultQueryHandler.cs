
using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;


namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class FeedbackResultQueryHandler : IRequestHandler<FeedbackResultQuery, EmployerFeedbackResultDto>
    {
        private readonly ILogger<FeedbackResultQueryHandler> _logger;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        public FeedbackResultQueryHandler(IEmployerFeedbackOuterApi employerFeedbackOuterApi ,ILogger<FeedbackResultQueryHandler> logger)
        {
            _employerFeedbackOuterApi = employerFeedbackOuterApi;
            _logger = logger;
        }
        public async Task<EmployerFeedbackResultDto> Handle(FeedbackResultQuery request, CancellationToken token)
        {
            IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerFeedbackOuterApi.GetFeedbackResultSummary(request.Ukprn);

            if (feedback == null || !feedback.Any())
            {
                return new EmployerFeedbackResultDto()
                {
                    Ukprn = request.Ukprn,
                    ProviderAttribute = Enumerable.Empty<ProviderAttributeSummaryItemDto>()
                };
            }

            IEnumerable<EmployerFeedbackResultDto> grouped = feedback.GroupBy(
                x => new { x.Ukprn, x.Stars, x.ReviewCount },
                x => new ProviderAttributeSummaryItemDto
                {
                    Name = x.AttributeName,
                    Strength = x.Strength,
                    Weakness = x.Weakness
                },
                (t, f) => new EmployerFeedbackResultDto
                {
                    Ukprn = t.Ukprn,
                    Stars = t.Stars,
                    ReviewCount = t.ReviewCount,
                    ProviderAttribute = f
                });

            return grouped.FirstOrDefault();
        }
    }
}
