using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Infrastructure
{
    public class EnsureSessionExistsAttributeTests
    {
        private readonly ProviderController _controller;
        private readonly Mock<ISessionStorageService> _sessionServiceMock;
        private readonly Mock<ITrainingProviderService> _iTrainingProviderServiceMock;
        private readonly Mock<ILogger<ProviderController>> _controllerLoggerMock;
        private readonly Mock<ILogger<EnsureSessionExists>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();
        private readonly Mock<IUserService> _userServiceMock;

        public EnsureSessionExistsAttributeTests()
        {
            _controllerLoggerMock = new Mock<ILogger<ProviderController>>();
            _loggerMock = new Mock<ILogger<EnsureSessionExists>>();
            _sessionServiceMock = new Mock<ISessionStorageService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new ProviderController(
                            _sessionServiceMock.Object,
                            null,
                            _controllerLoggerMock.Object,
                            null,
                            null,
                            _userServiceMock.Object);
            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(EmployerClaims.UserId, "TestUserIdValue"),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        [Test]
        public void Session_NotExists_Should_RedirectToLanding()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = httpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
               _controller);

            var ensureSession = new EnsureSessionExists(_sessionServiceMock.Object, _loggerMock.Object, _userServiceMock.Object);

            // Act
            ensureSession.OnActionExecuting(context);
            // Assert
            context
                  .Result
                  .Should()
                  .NotBeNull()
                  .And
                  .BeAssignableTo<RedirectToRouteResult>()
                  .Which
                .RouteName
                .Should()
                .BeEquivalentTo(RouteNames.Landing_Get);
        }

        [OneTimeTearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }
    }
}
