using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserAccounts;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization;
using SFA.DAS.Testing.AutoFixture;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.EmployerFeedback.Infrastructure.Configuration.EmployerClaims;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Services
{
    public class EmployerRoleAuthorizationServiceTests
    {
        [Test]
        [MoqInlineAutoData(UserRole.Owner, true, "Owner")]
        [MoqInlineAutoData(UserRole.Owner, false, "Transactor")]
        [MoqInlineAutoData(UserRole.Owner, false, "Viewer")]
        [MoqInlineAutoData(UserRole.Owner, false, "None")]
        [MoqInlineAutoData(UserRole.Owner, false, "")]
        [MoqInlineAutoData(UserRole.Transactor, true, "Owner")]
        [MoqInlineAutoData(UserRole.Transactor, true, "Transactor")]
        [MoqInlineAutoData(UserRole.Transactor, false, "Viewer")]
        [MoqInlineAutoData(UserRole.Transactor, false, "None")]
        [MoqInlineAutoData(UserRole.Transactor, false, "")]
        [MoqInlineAutoData(UserRole.Viewer, true, "Owner")]
        [MoqInlineAutoData(UserRole.Viewer, true, "Transactor")]
        [MoqInlineAutoData(UserRole.Viewer, true, "Viewer")]
        [MoqInlineAutoData(UserRole.Viewer, false, "None")]
        [MoqInlineAutoData(UserRole.Viewer, false, "")]
        [MoqInlineAutoData(UserRole.None, true, "Owner")]
        [MoqInlineAutoData(UserRole.None, true, "Transactor")]
        [MoqInlineAutoData(UserRole.None, true, "Viewer")]
        [MoqInlineAutoData(UserRole.None, true, "None")]
        [MoqInlineAutoData(UserRole.None, false, "")]
        public async Task Given_RouteAccountInAccountsClaim_And_RoleOnClaimIsSameOrHigher_Then_ReturnsTrue_Otherwise_ReturnsFalse(
            UserRole minimumRole,
            bool accessResult,
            string roleInClaims,
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = roleInClaims;

            var claimsPrinciple = GetClaims("userId", "email", employerUserAccount);

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerUserAccount.AccountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, minimumRole);

            // Assert
            actual.Should().Be(accessResult);
        }

        [Test, MoqAutoData]
        public async Task Given_RouteAccountNotInAccountsClaim_And_AccountsClaimRefreshed_WhenUserHasNoRole_Then_ReturnsFalse(
            string accountId,
            string userId,
            string email,
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IGovAuthEmployerAccountService> userAccountsService,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = "Owner";

            userAccountsService.Setup(x => x.GetUserAccounts(userId, email))
                .ReturnsAsync(new EmployerUserAccounts()
                {
                    EmployerAccounts = new List<EmployerUserAccountItem>
                    {
                        employerUserAccount // user has no role for route accountId
                    }
                });

            var claimsPrinciple = GetClaims(userId, email, employerUserAccount);

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Owner);

            // Assert
            actual.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Given_RouteAccountNotInAccountsClaim_And_AccountsClaimRefreshed_WhenUserHasRole_Then_ReturnsTrue(
            string accountId,
            string userId,
            string email,
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            [Frozen] Mock<IGovAuthEmployerAccountService> userAccountsService,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = "Owner";

            userAccountsService.Setup(x => x.GetUserAccounts(userId, email))
                .ReturnsAsync(new EmployerUserAccounts
                {
                    EmployerAccounts = new List<EmployerUserAccountItem>
                    {
                        new EmployerUserAccountItem
                        {
                            AccountId = accountId.ToUpper(), // user has Owner role for route accountId
                            Role = employerUserAccount.Role
                        }
                    }
                });

            var claimsPrinciple = GetClaims(userId, email, employerUserAccount);

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            // Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Owner);

            // Assert
            actual.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Given_AccountId_NotInUrl_Then_ReturnsFalse(
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.Role = "Owner";
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();

            var claimsPrinciple = GetClaims("userId", "email", employerUserAccount);

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Clear(); // do not put accountId in Url
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Viewer);

            //Assert
            actual.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Given_NoMatchingAccountsIdentifierClaimFound_Then_ReturnsFalse(
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = "Owner";

            var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerUserAccount.AccountId, employerUserAccount } };
            var employerAccountClaim = new Claim("Not_UserAssociatedAccountsClaimsTypeIdentifier", JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[]
            {
                employerAccountClaim,
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim(ClaimTypes.Email, "email") })
            });

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, employerUserAccount.AccountId);
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Viewer);

            //Assert
            actual.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Given_RouteAccountNotInAccountsClaim_And_NoMatchingNameIdentifierClaimFound_ThenReturnsFalse(
            string accountId,
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = "Owner";

            var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerUserAccount.AccountId, employerUserAccount } };
            var employerAccountClaim = new Claim(EmployerClaims.UserAssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[]
            {
                employerAccountClaim,
                new Claim("Not_NameIdentifier", "userId"),
                new Claim(ClaimTypes.Email, "email") })
            });

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Viewer);

            //Assert
            actual.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Given_RouteAccountNotInAccountsClaim_And_NoMatchingEmailClaimFound_ThenReturnsFalse(
            string accountId,
            EmployerUserAccountItem employerUserAccount,
            [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
            EmployerRoleAuthorizationService employerRoleAuthorizationService)
        {
            // Arrange
            employerUserAccount.AccountId = employerUserAccount.AccountId.ToUpper();
            employerUserAccount.Role = "Owner";

            var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerUserAccount.AccountId, employerUserAccount } };
            var employerAccountClaim = new Claim(EmployerClaims.UserAssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));
            var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[]
            {
                employerAccountClaim,
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("Not_Email", "email") })
            });

            var httpContext = new DefaultHttpContext(new FeatureCollection());
            httpContext.Request.RouteValues.Add(RouteValueKeys.HashedAccountId, accountId.ToUpper());
            httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            //Act
            var actual = await employerRoleAuthorizationService.IsEmployerAuthorized(claimsPrinciple, UserRole.Viewer);

            //Assert
            actual.Should().BeFalse();
        }

        private ClaimsPrincipal GetClaims(string userId, string email, EmployerUserAccountItem employerUserAccount)
        {
            var employerAccounts = new Dictionary<string, EmployerUserAccountItem> { { employerUserAccount.AccountId, employerUserAccount } };
            var employerAccountClaim = new Claim(EmployerClaims.UserAssociatedAccountsClaimsTypeIdentifier, JsonConvert.SerializeObject(employerAccounts));

            return new ClaimsPrincipal(new[] { new ClaimsIdentity(new[]
            {
                employerAccountClaim,
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email) })
            });
        }
    }
}