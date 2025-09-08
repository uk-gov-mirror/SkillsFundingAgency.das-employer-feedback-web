using AutoFixture.NUnit3;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Interfaces;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.EmployerAccount;

namespace UnitTests.ApplicationServices
{
    public class WhenGettingUserAccounts
    {
        [Test, MoqAutoData]
        public async Task Then_The_Api_Is_Called_And_Data_Returned(
            string email,
            string userId,
            UserAccountsDetails response,
            [Frozen] Mock<IOuterApiClient> apiClient,
            EmployerAccountService service)
        {
            // Arrange
            var expectedRequest = new GetUserAccountsRequest(userId, email);
            apiClient.Setup(x =>
                    x.Get<UserAccountsDetails>(
                        It.Is<GetUserAccountsRequest>(c => c.GetUrl.Equals(expectedRequest.GetUrl))))
                .ReturnsAsync(new ApiResponse<UserAccountsDetails>(response, HttpStatusCode.OK, ""));

            // Act
            var actual = await service.GetUserAccounts(userId, email);

            // Assert
            actual.Should().BeEquivalentTo(new
            {
                EmployerAccounts = response.UserAccounts != null
                    ? response.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = response.FirstName,
                IsSuspended = response.IsSuspended,
                LastName = response.LastName,
                EmployerUserId = response.EmployerUserId,
            });
        }
    }
}