using System.Web;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests{
    public class GetUserAccountsRequest
    {
        public string _userId { get; set; }
        public string _email { get; set; }

    }
}