using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.EmployerAccount
{
    public class EmployerAccountService : IGovAuthEmployerAccountService
    {
        private readonly IEmployerFeedbackOuterApi _apiClient;

        public EmployerAccountService(IEmployerFeedbackOuterApi apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
        {
            var result = await _apiClient.GetUserAccounts(userId, email);

            return new EmployerUserAccounts
            {
                EmployerAccounts = result.UserAccounts != null
                    ? result.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = result.FirstName,
                IsSuspended = result.IsSuspended,
                LastName = result.LastName,
                EmployerUserId = result.EmployerUserId,
            };
        }
    }
}