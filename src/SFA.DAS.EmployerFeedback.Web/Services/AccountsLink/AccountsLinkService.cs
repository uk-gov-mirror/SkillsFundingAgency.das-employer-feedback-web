using SFA.DAS.Employer.Shared.UI;

namespace SFA.DAS.EmployerFeedback.Web.Services
{
    public class AccountsLinkService : IAccountsLinkService
    {
        private readonly UrlBuilder _urlBuilder;
        public AccountsLinkService(UrlBuilder urlBuilder) => _urlBuilder = urlBuilder;
        public string AccountsHome(string encodedAccountId) =>
            _urlBuilder.AccountsLink("AccountsHome", encodedAccountId);
    }

}
