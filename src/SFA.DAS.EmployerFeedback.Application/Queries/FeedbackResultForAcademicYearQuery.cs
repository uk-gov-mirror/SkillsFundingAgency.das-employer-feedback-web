using MediatR;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackResultForAcademicYearQuery : IRequest<EmployerFeedbackForAcademicYearResultDto>
    {
        public long Ukprn { get; set; }
        public string  AcademicYear { get; set; }
    }
}
