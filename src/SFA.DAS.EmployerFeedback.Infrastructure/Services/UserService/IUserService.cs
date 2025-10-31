using System;

namespace SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService
{
    public interface IUserService
    {
        Guid? GetUserId();
    }
}