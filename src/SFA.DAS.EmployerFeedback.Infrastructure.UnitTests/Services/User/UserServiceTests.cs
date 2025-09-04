using NUnit.Framework;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserAccounts;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.EmployerFeedback.Infrastructure.Configuration.EmployerClaims;

namespace SFA.DAS.EmployerFeedback.Infrastructure.UnitTests.Services.User
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private UserService _userService;
        private ClaimsIdentity _identity;
        private Mock<HttpContext> _mockHttpContext;

        [SetUp]
        public void Setup()
        {
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _identity = new ClaimsIdentity("TestAuthenticationType");  // Simulates an authenticated user
            _mockHttpContext.Setup(x => x.User).Returns(new ClaimsPrincipal(_identity));
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            _userService = new UserService(_mockHttpContextAccessor.Object);
        }

        [Test]
        public void GetUserId_ShouldReturnUserId_WhenUserIsAuthenticated()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            _identity.AddClaim(new Claim(EmployerClaims.UserId, userId));

            // Act
            var result = _userService.GetUserId();

            // Assert
            result.Should().Be(userId);
        }

        [Test]
        public void IsUserChangeAuthorized_ShouldReturnTrue_ForOwnerOrTransactor()
        {
            // Arrange
            var accountId = "account123";
            var claimsData = new Dictionary<string, EmployerUserAccountItem>
            {
                { accountId, new EmployerUserAccountItem { Role = "Owner" } }
            };
            
            var claimsJson = JsonConvert.SerializeObject(claimsData);
            _identity.AddClaim(new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, claimsJson));

            // Act
            var result = _userService.IsUserChangeAuthorized(accountId);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUserChangeAuthorized_ShouldReturnFalse_WhenUserIsNotAuthorized()
        {
            // Arrange
            var accountId = "account123";
            var claimsData = new Dictionary<string, EmployerUserAccountItem>
            {
                { accountId, new EmployerUserAccountItem { Role = "Viewer" } }
            };
            
            var claimsJson = JsonConvert.SerializeObject(claimsData);
            _identity.AddClaim(new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, claimsJson));

            // Act
            var result = _userService.IsUserChangeAuthorized(accountId);

            // Assert
            result.Should().BeFalse();
        }
    }
}
