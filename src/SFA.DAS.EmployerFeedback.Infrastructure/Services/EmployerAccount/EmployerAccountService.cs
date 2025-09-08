using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.GovUK.Auth.Employer;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.EmployerAccount
{   
    public class EmployerAccountService : IGovAuthEmployerAccountService
    {
        private readonly IOuterApiClient _apiClient;
        
        public EmployerAccountService(IOuterApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
        {
            var result = await _apiClient.Get<UserAccountsDetails>(new GetUserAccountsRequest(userId, email));

            return new EmployerUserAccounts
            {
                EmployerAccounts = result.Body.UserAccounts != null
                    ? result.Body.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = result.Body.FirstName,
                IsSuspended = result.Body.IsSuspended,
                LastName = result.Body.LastName,
                EmployerUserId = result.Body.EmployerUserId,
            };
        }
    }
}