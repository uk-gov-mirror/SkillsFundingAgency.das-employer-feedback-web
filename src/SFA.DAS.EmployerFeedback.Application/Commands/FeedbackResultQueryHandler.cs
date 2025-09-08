
using Microsoft.Extensions.Logging;
using MediatR;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;


namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class FeedbackResultQueryHandler : IRequestHandler<FeedbackResultQuery, EmployerFeedbackResultDto>
    {
        private readonly ILogger<FeedbackResultQueryHandler> _logger;

        public FeedbackResultQueryHandler(ILogger<FeedbackResultQueryHandler> logger)
        {
            _logger = logger;
        }
        public async Task<EmployerFeedbackResultDto> Handle(FeedbackResultQuery request, CancellationToken token)
        {
            //FIXME - replace call with outer api call
            //IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetFeedbackResultSummary(request.Ukprn);

            //if (feedback == null || !feedback.Any())
            //{
            //    return new EmployerFeedbackResultDto()
            //    {
            //        Ukprn = request.Ukprn,
            //        ProviderAttribute = Enumerable.Empty<ProviderAttributeSummaryItemDto>()
            //    };
            //}

            //IEnumerable<EmployerFeedbackResultDto> grouped = feedback.GroupBy(
            //    x => new { x.Ukprn, x.Stars, x.ReviewCount },
            //    x => new ProviderAttributeSummaryItemDto
            //    {
            //        Name = x.AttributeName,
            //        Strength = x.Strength,
            //        Weakness = x.Weakness
            //    },
            //    (t, f) => new EmployerFeedbackResultDto
            //    {
            //        Ukprn = t.Ukprn,
            //        Stars = t.Stars,
            //        ReviewCount = t.ReviewCount,
            //        ProviderAttribute = f
            //    });

            //return grouped.FirstOrDefault();
            throw new NotImplementedException();
        }
    }
}
