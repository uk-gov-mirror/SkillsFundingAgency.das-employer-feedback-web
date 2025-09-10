using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration.Routing;
using SFA.DAS.EmployerProvideFeedback.Infrastructure;
using SFA.DAS.Encoding;

namespace UnitTests.EmployerProvideFeedback.Infrastructure
{
    public class EnsureFeedbackNotSubmittedAttributeTests
    {
        private readonly Mock<Controller> _controllerMock;
        private readonly Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApi;
        private readonly Mock<IEncodingService> _encodingServiceMock;

        public EnsureFeedbackNotSubmittedAttributeTests()
        {
            _controllerMock = new Mock<Controller>();
            _controllerMock.Setup(mock => mock.RedirectToRoute(It.IsAny<string>(), It.IsAny<object>())).Returns(new RedirectToRouteResult(RouteNames.FeedbackAlreadySubmitted, new { encodedAccountId = "ABCDEF" }));
            _encodingServiceMock = new Mock<IEncodingService>();
            _encodingServiceMock.Setup(m => m.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns("ABCDEF");
            _employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
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
               _controllerMock.Object);
            context.ActionArguments.Add("uniqueCode", Guid.NewGuid());

            var ensureSession = new EnsureFeedbackNotSubmitted(_employerFeedbackOuterApi.Object, _encodingServiceMock.Object);

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
    }
}
