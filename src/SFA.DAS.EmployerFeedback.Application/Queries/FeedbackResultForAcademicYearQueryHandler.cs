using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class FeedbackResultForAcademicYearQueryHandler : IRequestHandler<FeedbackResultForAcademicYearQuery, EmployerFeedbackForAcademicYearResultDto>
    {
        private readonly ILogger<FeedbackResultForAcademicYearQueryHandler> _logger;

        public FeedbackResultForAcademicYearQueryHandler(ILogger<FeedbackResultForAcademicYearQueryHandler> logger)
        {
            _logger = logger;
        }
        public async Task<EmployerFeedbackForAcademicYearResultDto> Handle(FeedbackResultForAcademicYearQuery request, CancellationToken token)
        {
            //FIXME - replace call with outer api call
            //IEnumerable<EmployerFeedbackResultSummary> feedback = await _employerfeedbackRepository.GetFeedbackResultSummaryForAcademicYear(request.Ukprn, request.AcademicYear);

            //if (feedback == null || !feedback.Any())
            //{
            //    return new EmployerFeedbackForAcademicYearResultDto()
            //    {
            //        Ukprn = request.Ukprn,
            //        ProviderAttribute = Enumerable.Empty<ProviderAttributeForAcademicYearSummaryItemDto>()
            //    };
            //}

            //IEnumerable<EmployerFeedbackForAcademicYearResultDto> grouped = feedback.GroupBy(
            //    x => new { x.Ukprn, x.Stars, x.ReviewCount,x.TimePeriod },
            //    x => new ProviderAttributeForAcademicYearSummaryItemDto
            //    {
            //        Name = x.AttributeName,
            //        Strength = x.Strength,
            //        Weakness = x.Weakness,
            //    },
            //    (t, f) => new EmployerFeedbackForAcademicYearResultDto
            //    {
            //        Ukprn = t.Ukprn,
            //        Stars = t.Stars,
            //        ReviewCount = t.ReviewCount,
            //        TimePeriod = t.TimePeriod,
            //        ProviderAttribute = f
            //    });

            //return grouped.FirstOrDefault();
            throw new NotImplementedException();
        }
    }
}
