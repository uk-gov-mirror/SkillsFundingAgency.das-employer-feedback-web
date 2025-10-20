using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService
{
    public interface IUserService
    {
        Guid? GetUserId();

        string GetUserDisplayName();

        bool IsUserChangeAuthorized(string accountId);

        IEnumerable<string> GetUserOwnerTransactorAccountIds();

        bool IsOwnerOrTransactor(string accountId);
    }
}