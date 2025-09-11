using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Commands
{
    public class FeedbackQueryHandler : IRequestHandler<FeedbackQuery, IEnumerable<EmployerFeedbackDto>>
    {
        private readonly ILogger<FeedbackQueryHandler> _logger;
        private readonly IEmployerFeedbackOuterApi _employerFeedbackOuterApi;

        public FeedbackQueryHandler(IEmployerFeedbackOuterApi employerfeedbackRepository, ILogger<FeedbackQueryHandler> logger)
        {
            _employerFeedbackOuterApi = employerfeedbackRepository;
            _logger = logger;
        }
        public async Task<IEnumerable<EmployerFeedbackDto>> Handle(FeedbackQuery request, CancellationToken token)
        {

            var feedback = await _employerFeedbackOuterApi.GetEmployerFeedback();

            if (feedback == null || !feedback.Any())
            {
                return Enumerable.Empty<EmployerFeedbackDto>();
            }

            var groupedFeedback = feedback.GroupBy(
                x => new { x.Id, x.Ukprn, x.DateTimeCompleted, x.ProviderRating },
                x => new ProviderAttributeDto
                {
                    Name = x.AttributeName,
                    Value = x.AttributeValue
                },
                (t, f) => new EmployerFeedbackDto
                {
                    DateTimeCompleted = t.DateTimeCompleted,
                    ProviderRating = t.ProviderRating,
                    Ukprn = t.Ukprn,
                    ProviderAttributes = new List<ProviderAttributeDto>(f.Where(s => s.Name != null))
                });

            return groupedFeedback;
        }
    }
}
