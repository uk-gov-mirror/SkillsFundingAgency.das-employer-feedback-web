using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using RestEase;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserAccounts;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Testing.AutoFixture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerRequestApprenticeTraining.Infrastructure.UnitTests.Services.UserAccounts
{
    public class UserAccountsServiceTests
    {
        [Test, MoqAutoData]
        public async Task Given_GetUserAccountsCalled_Then_ResponseReturnedForUser(
            string userId,
            string email,
            [Frozen] Mock<IEmployerFeedbackOuterApi> outerApiMock,
            UserAccountsService userAccountsService)
        {
            // Arrange
            var userAccountsResponse = new UserAccountsDetails
            {
                EmployerUserId = userId,
                FirstName = "First",
                LastName = "Last",
                IsSuspended = false,
                UserAccounts = new List<EmployerIdentifier>
                {
                    new EmployerIdentifier
                    {
                        AccountId = "ABC123",
                        EmployerName = "Primary",
                        Role = "Owner"
                    }
                }
            };

            outerApiMock.Setup(p => p.GetUserAccounts(userId, email))
                .ReturnsAsync(userAccountsResponse);

            // Act
            var actual = await userAccountsService.GetUserAccounts(userId, email);

            // Assert
            actual.Should().BeEquivalentTo(new
            {
                EmployerAccounts = userAccountsResponse.UserAccounts != null
                    ? userAccountsResponse.UserAccounts.Select(c => new EmployerUserAccountItem
                    {
                        Role = c.Role,
                        AccountId = c.AccountId,
                        ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                        EmployerName = c.EmployerName,
                    }).ToList()
                    : [],
                FirstName = userAccountsResponse.FirstName,
                IsSuspended = userAccountsResponse.IsSuspended,
                LastName = userAccountsResponse.LastName,
                EmployerUserId = userAccountsResponse.EmployerUserId,
            });
        }

        [Test, MoqAutoData]
        public async Task Given_GetUserAccountsCalled_AndExceptionThrown_Then_NoResponseReturnedForUser(
            string userId,
            string email,
            [Frozen] Mock<IEmployerFeedbackOuterApi> outerApiMock,
            UserAccountsService userAccountsService)
        {
            // Arrange
            var userAccountsResponse = new UserAccountsDetails();

            outerApiMock.Setup(p => p.GetUserAccounts(userId, email))
                .Throws(new ApiException(new HttpRequestMessage(), new HttpResponseMessage(), string.Empty));

            // Act
            var actual = await userAccountsService.GetUserAccounts(userId, email);

            // Assert
            actual.Should().BeEquivalentTo(new EmployerUserAccounts());
        }
    }
}