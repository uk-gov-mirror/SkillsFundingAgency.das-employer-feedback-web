using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using System.Web;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests{
    public class GetUserAccountsRequest : IGetApiRequest
    {
        private readonly string _userId;
        private readonly string _email;

        public GetUserAccountsRequest(string userId, string email)
        {
            _userId = userId;
            _email = HttpUtility.UrlEncode(email);
        }

        public string GetUrl => $"accountusers/{_userId}/accounts?email={_email}";
    }
}