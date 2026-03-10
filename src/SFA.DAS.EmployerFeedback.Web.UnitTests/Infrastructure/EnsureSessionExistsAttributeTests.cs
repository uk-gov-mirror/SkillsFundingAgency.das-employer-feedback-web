using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Infrastructure
{
    [TestFixture]
    public class EnsureSessionExistsAttributeTests
    {
        private Mock<ISessionService> _session;
        private Mock<ILogger<EnsureSessionExistsAttribute>> _sessionLogger;
        private Mock<IUserService> _userService;

        private EnsureSessionExistsAttribute _sut;

        [SetUp]
        public void SetUp()
        {
            _session = new Mock<ISessionService>(MockBehavior.Strict);
            _sessionLogger = new Mock<ILogger<EnsureSessionExistsAttribute>>();
            _userService = new Mock<IUserService>(MockBehavior.Strict);

            _sut = new EnsureSessionExistsAttribute(_session.Object, _sessionLogger.Object, _userService.Object);
        }

        private ActionExecutingContext BuildContext(Controller controller = null)
        {
            var http = new DefaultHttpContext();
            var actionCtx = new ActionContext(
                http,
                new RouteData(),
                new ControllerActionDescriptor { ControllerName = "Minimal", ActionName = "Any" });

            var ctrl = controller ?? new MinimalController();

            return new ActionExecutingContext(
                actionCtx,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                ctrl);
        }

        private static (ActionExecutionDelegate next, Task signal) NextDelegate()
        {
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            ActionExecutionDelegate next = () =>
            {
                tcs.TrySetResult();
                var executedContext = new ActionExecutedContext(
                    new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
                    new List<IFilterMetadata>(),
                    new MinimalController());
                return Task.FromResult(executedContext);
            };

            return (next, tcs.Task);
        }

        [Test]
        public async Task When_UserId_Is_Missing_Should_Log_Warn_And_Redirect_To_ProviderSearchGet_And_Not_Call_Next()
        {
            // Arrange
            _userService.Setup(u => u.GetUserId()).Returns((Guid?)null);

            var context = BuildContext();
            var (next, signal) = NextDelegate();

            // Act
            await _sut.OnActionExecutionAsync(context, next);

            // Assert
            signal.IsCompleted.Should().BeFalse(); // next() not called

            context.Result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)context.Result;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);

            _sessionLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            _session.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_Survey_Is_Missing_Should_Log_Warn_And_Redirect_To_ProviderSearchGet_And_Not_Call_Next()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userService.Setup(u => u.GetUserId()).Returns(userId);
            _session
                .Setup(s => s.GetSurveyModel(userId))
                .ReturnsAsync((SurveyModel)null);

            var context = BuildContext();
            var (next, signal) = NextDelegate();

            // Act
            await _sut.OnActionExecutionAsync(context, next);

            // Assert
            signal.IsCompleted.Should().BeFalse(); // next() not called

            context.Result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)context.Result;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);

            _sessionLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);

            _session.Verify(s => s.GetSurveyModel(userId), Times.Once);
        }

        [Test]
        public async Task When_Survey_Exists_Should_Call_Next_And_Not_Set_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userService.Setup(u => u.GetUserId()).Returns(userId);
            _session.Setup(s => s.GetSurveyModel(userId)).ReturnsAsync(new SurveyModel());

            var context = BuildContext();
            var (next, signal) = NextDelegate();

            // Act
            await _sut.OnActionExecutionAsync(context, next);

            // Assert
            signal.IsCompleted.Should().BeTrue(); // next() was called
            context.Result.Should().BeNull();

            _session.Verify(s => s.GetSurveyModel(userId), Times.Once);

            _sessionLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        private sealed class MinimalController : Controller
        {
        }
    }
}
