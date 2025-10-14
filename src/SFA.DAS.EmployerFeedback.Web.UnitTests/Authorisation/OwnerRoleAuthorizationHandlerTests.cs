using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Authorization;
using SFA.DAS.EmployerFeedback.Web.Services.EmployerRoleAuthorization;
using SFA.DAS.Testing.AutoFixture;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Authorization
{
    public class OwnerRoleAuthorizationHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Given_HandleRequirementAsync_And_UserIsAuthorized_SucceedsRequirement(
            [Frozen] Mock<IEmployerRoleAuthorizationService> employerRoleAuthorizationService)
        {
            // Arrange
            var requirement = new OwnerRoleRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Test User") }));
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            employerRoleAuthorizationService.Setup(x => x.IsEmployerAuthorized(It.IsAny<ClaimsPrincipal>(), UserRole.Owner))
                .ReturnsAsync(true);

            // Act
            var handler = new OwnerRoleAuthorizationHandler(employerRoleAuthorizationService.Object);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeTrue();
        }

        [Test, MoqAutoData]
        public async Task Given_HandleRequirementAsyncIsCalled_And_UserIsNotAuthorized_FailsRequirement(
            [Frozen] Mock<IEmployerRoleAuthorizationService> employerRoleAuthorizationService)
        {
            // Arrange
            var requirement = new OwnerRoleRequirement();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Test User") }));
            var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

            employerRoleAuthorizationService.Setup(x => x.IsEmployerAuthorized(It.IsAny<ClaimsPrincipal>(), UserRole.Owner))
                .ReturnsAsync(false);

            // Act
            var handler = new OwnerRoleAuthorizationHandler(employerRoleAuthorizationService.Object);

            // Act
            await handler.HandleAsync(context);

            // Assert
            context.HasSucceeded.Should().BeFalse();
        }
    }
}
