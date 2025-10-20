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
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Filters
{
    [TestFixture]
    public class EnsureSessionExistsAttributeTests
    {
        private Mock<ISessionStorageService> _session;
        private Mock<ITrainingProviderService> _trainingProviderService;
        private Mock<IEmployerFeedbackOuterApi> _outerApi;
        private Mock<IAccountsLinkService> _accountsLinkService;
        private Mock<IUserService> _userService;
        
        private Mock<ILogger<EnsureSessionExistsAttribute>> _sessionLogger;
        private Mock<ILogger<ProviderController>> _trainingProviderLogger;

        private EnsureSessionExistsAttribute _sut;

        [SetUp]
        public void SetUp()
        {
            _session = new Mock<ISessionStorageService>(MockBehavior.Strict);
            _trainingProviderService = new Mock<ITrainingProviderService>();
            _outerApi = new Mock<IEmployerFeedbackOuterApi>();
            _accountsLinkService = new Mock<IAccountsLinkService>();
            _userService = new Mock<IUserService>(MockBehavior.Strict);

            _sessionLogger = new Mock<ILogger<EnsureSessionExistsAttribute>>();
            _trainingProviderLogger = new Mock<ILogger<ProviderController>>();

            _sut = new EnsureSessionExistsAttribute(_session.Object, _sessionLogger.Object, _userService.Object);
        }

        private ActionExecutingContext BuildContext(Controller controller = null)
        {
            var http = new DefaultHttpContext();
            var actionCtx = new ActionContext(
                http,
                new RouteData(),
                new ControllerActionDescriptor { ControllerName = "Provider", ActionName = "Any" });

            var ctrl = controller ?? new ProviderController(_session.Object, _trainingProviderService.Object, 
                _trainingProviderLogger.Object, _outerApi.Object, _accountsLinkService.Object, _userService.Object);

            return new ActionExecutingContext(
                actionCtx,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                ctrl);
        }

        private ActionExecutionDelegate NextDelegate(out bool called)
        {
            called = false;
            return () =>
            {
                var executedContext = new ActionExecutedContext(
                    new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
                    new List<IFilterMetadata>(),
                    new ProviderController(_session.Object, _trainingProviderService.Object,
                        _trainingProviderLogger.Object, _outerApi.Object, _accountsLinkService.Object, _userService.Object));
                return Task.FromResult(executedContext);
            };
        }

        [Test]
        public async Task When_UserId_Is_Missing_Should_Log_Warn_And_Redirect_To_ProviderSearchGet_And_Not_Call_Next()
        {
            // Arrange
            _userService.Setup(u => u.GetUserId()).Returns((Guid?)null);

            var context = BuildContext();
            var next = NextDelegate(out var nextCalled);

            // Act
            await _sut.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.Should().BeFalse();

            context.Result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)context.Result;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);

            _sessionLogger.Verify(l =>
                l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(o => o.ToString().Contains("No survey was started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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
            var next = NextDelegate(out var nextCalled);

            // Act
            //await _sut.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.Should().BeFalse();

            context.Result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = (RedirectToRouteResult)context.Result;
            redirect.RouteName.Should().Be(ProviderController.ProviderSearchGet);

            _sessionLogger.Verify(l =>
                l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(o => o.ToString().Contains("No survey was started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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
            var next = NextDelegate(out var nextCalled);

            // Act
            await _sut.OnActionExecutionAsync(context, next);

            // Assert
            nextCalled.Should().BeTrue();
            context.Result.Should().BeNull();

            _session.Verify(s => s.GetSurveyModel(userId), Times.Once);
            // No warning should be logged
            _sessionLogger.Verify(l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
