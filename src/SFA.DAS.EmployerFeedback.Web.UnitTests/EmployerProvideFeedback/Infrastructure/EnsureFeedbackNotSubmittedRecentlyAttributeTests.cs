using System;
using System.Collections.Generic;
using System.Security.Claims;
using SFA.DAS.EmployerProvideFeedback.Infrastructure;
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
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.Encoding;
using SFA.DAS.EmployerFeedback.Infrastructure;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedRecentlyAttributeTests
    {
        private readonly HomeController _controller;
        private readonly Mock<ISessionStorageService> _sessionServiceMock;
        private readonly Mock<IEncodingService> _encodingServiceMock;
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock = new Mock<IConfiguration>();
        private readonly Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;

        public EnsureFeedbackNotSubmittedRecentlyAttributeTests()
        {
            _sessionServiceMock = new Mock<ISessionStorageService>();
            _encodingServiceMock = new Mock<IEncodingService>();
            _loggerMock = new Mock<ILogger<HomeController>>();
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();

            _controller = new HomeController(
                            _sessionServiceMock.Object,
                            _encodingServiceMock.Object,
                            _loggerMock.Object,
                            _configurationMock.Object,
                            null,
                            null,
                            _employerFeedbackOuterApi.Object);

            var context = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "TestUserIdValue"),
                }))
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
        }

        [Test]
        public void When_Feedback_Submitted_Recently_Then_Redirect_To_FeedbackAlreadySubmitted()
        {
            // Arrange

            var config = new EmployerFeedbackWebConfiguration()
            {
                // Configure "recently" to be 10 days ago
                FeedbackWaitPeriodDays = 10
            };


            // Set feedback given 5 days ago

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

            var ensureSession = new EnsureFeedbackNotSubmittedRecentlyAttribute(_employerFeedbackOuterApi.Object,config);

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
                .BeEquivalentTo(RouteNames.FeedbackAlreadySubmitted);
        }

        [Test]
        public void When_Feedback_Not_Submitted_Recently_Then_No_Redirect()
        {
            // Arrange

            var config = new EmployerFeedbackWebConfiguration()
            {
                // Configure "recently" to be 10 days ago
                FeedbackWaitPeriodDays = 10
            };

            var sessionServiceMock = new Mock<IEmployerFeedbackOuterApi>();
            // Set feedback given 15 days ago

            var context = new ActionExecutingContext(
                new ActionContext
                {
                    HttpContext = _controller.HttpContext,
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                },
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
               _controller);
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureFeedbackNotSubmittedRecentlyAttribute(_employerFeedbackOuterApi.Object,config);

            // Act
            ensureSession.OnActionExecuting(context);

            // Assert
            context
                .Result
                .Should()
                .BeNull();
        }

        [OneTimeTearDown]
        public void DisposeController()
        {
            _controller.Dispose();
        }
    }
}
