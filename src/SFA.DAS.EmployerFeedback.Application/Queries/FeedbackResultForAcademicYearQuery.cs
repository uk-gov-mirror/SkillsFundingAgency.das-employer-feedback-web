using MediatR;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;

namespace SFA.DAS.EmployerFeedback.Application.Queries
{
    public class FeedbackResultForAcademicYearQuery : IRequest<EmployerFeedbackForAcademicYearResultDto>
    {
        public long Ukprn { get; set; }
        public string  AcademicYear { get; set; }
    }
}
