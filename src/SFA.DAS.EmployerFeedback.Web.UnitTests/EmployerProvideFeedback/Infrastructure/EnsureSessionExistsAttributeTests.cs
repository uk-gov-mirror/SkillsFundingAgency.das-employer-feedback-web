using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Infrastructure
{
    public class EnsureSessionExistsAttributeTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ISessionStorageService> _sessionServiceMock;
        private readonly Mock<ILogger<HomeController>> _controllerLoggerMock;
        private readonly Mock<ILogger<EnsureSessionExists>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();

        public EnsureSessionExistsAttributeTests()
        {
            _controllerLoggerMock = new Mock<ILogger<HomeController>>();
            _loggerMock = new Mock<ILogger<EnsureSessionExists>>();
            _sessionServiceMock = new Mock<ISessionStorageService>();

            _controller = new HomeController(
                            _sessionServiceMock.Object,
                            _controllerLoggerMock.Object,
                            _configurationMock.Object,
                            null,
                            null);
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
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureSessionExists(_sessionServiceMock.Object, _loggerMock.Object);

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
