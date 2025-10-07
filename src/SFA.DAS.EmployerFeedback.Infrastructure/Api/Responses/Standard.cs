using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses
{

    [ExcludeFromCodeCoverage]
    public class Standard
    {
        public string StandardReference { get; set; }
        public string StandardTitle { get; set; }
        public string StandardSector { get; set; }
        public int StandardLevel { get; set; }
    }
}
