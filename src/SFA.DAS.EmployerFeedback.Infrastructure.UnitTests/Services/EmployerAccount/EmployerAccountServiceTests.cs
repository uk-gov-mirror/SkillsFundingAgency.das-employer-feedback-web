using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.EmployerAccount;
using SFA.DAS.GovUK.Auth.Employer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.UnitTests.Services.EmployerAccount
{
    [TestFixture]
    public class EmployerAccountServiceTests
    {
        private Mock<IEmployerFeedbackOuterApi> _apiClientMock;
        private EmployerAccountService _service;

        [SetUp]
        public void Arrange()
        {
            _apiClientMock = new Mock<IEmployerFeedbackOuterApi>();
            _service = new EmployerAccountService(_apiClientMock.Object);
        }

        [Test]
        public async Task GetUserAccounts_ReturnsUserAccounts()
        {
            // Arrange
            var userId = "test-user-id";
            var email = "test@test.com";

            var expectedResponse = new EmployerUserAccounts
            {
                EmployerUserId = "test-user-id",
                FirstName = "Test",
                LastName = "User",
                IsSuspended = false,
                EmployerAccounts = new System.Collections.Generic.List<EmployerUserAccountItem>
                {
                    new EmployerUserAccountItem
                    {
                        AccountId = "12345",
                        EmployerName = "Test Employer",
                        Role = "Owner",
                        ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                    }
                }
            };

            _apiClientMock.Setup(x => x.GetUserAccounts(userId, email))
                .ReturnsAsync(new UserAccountsDetails
                {
                    EmployerUserId = expectedResponse.EmployerUserId,
                    FirstName = expectedResponse.FirstName,
                    LastName = expectedResponse.LastName,
                    IsSuspended = expectedResponse.IsSuspended,
                    UserAccounts = new System.Collections.Generic.List<EmployerIdentifier>
                    {
                        new EmployerIdentifier
                        {
                            AccountId = "12345",
                            EmployerName = "Test Employer",
                            Role = "Owner",
                            ApprenticeshipEmployerType = ApprenticeshipEmployerType.Levy
                        }
                    }
                });

            var employerAccounts = await _service.GetUserAccounts(userId, email);


            // Assert
            Assert.That(employerAccounts != null);
            employerAccounts.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task GetUserAccounts_ReturnsEmptyAccounts_WhenMissingDetails()
        {
            // Arrange
            var userId = "test-user-id";
            var email = "test@test.com";

            var expectedResponse = new UserAccountsDetails
            {
                EmployerUserId = new Guid().ToString(),
                FirstName = null,
                LastName = null,
                IsSuspended = false,
                UserAccounts = new System.Collections.Generic.List<EmployerIdentifier>()
            };

            // Act
            _apiClientMock.Setup(x => x.GetUserAccounts(userId, email)).ReturnsAsync(expectedResponse);

            // Assert
            var employerAccounts = await _service.GetUserAccounts(userId, email);
            Assert.That(employerAccounts != null);
            Assert.That(employerAccounts.EmployerAccounts.ToList().Count == 0);
            Assert.That(employerAccounts.EmployerUserId == expectedResponse.EmployerUserId);
            Assert.That(employerAccounts.FirstName == expectedResponse.FirstName);
            Assert.That(employerAccounts.LastName == expectedResponse.LastName);
        }
    }
}
