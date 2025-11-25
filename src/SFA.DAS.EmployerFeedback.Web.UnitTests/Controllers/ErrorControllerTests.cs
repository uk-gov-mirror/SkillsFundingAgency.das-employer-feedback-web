using System;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.TestHelper.Extensions;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.Error;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<ILogger<ErrorController>> _mockLogger;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<HttpResponse> _mockResponse;
        private ErrorController _sut;

        [SetUp]
        public void Setup()
        {
            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            _mockLogger = new Mock<ILogger<ErrorController>>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockResponse = new Mock<HttpResponse>();

            _mockHttpContext.Setup(h => h.Response).Returns(_mockResponse.Object);
            _mockContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            _sut = new ErrorController(_mockContextAccessor.Object, _mockLogger.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = _mockHttpContext.Object }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void Error_Should_Log_When_Exception_Feature_Present()
        {
            // Arrange
            var feature = new Mock<IExceptionHandlerFeature>();
            feature.Setup(f => f.Error).Returns(new Exception("Test exception"));

            var features = new FeatureCollection();
            features.Set(feature.Object);
            _mockHttpContext.Setup(h => h.Features).Returns(features);

            // Act
            var result = _sut.Error(null);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _mockLogger.VerifyLogError("Unhandled exception", Times.Once);
        }

        [Test]
        public void Error_Should_Return_PageNotFound_View_When_Id_Is_404()
        {
            // Arrange
            var features = new FeatureCollection();
            _mockHttpContext.Setup(h => h.Features).Returns(features);

            // Act
            var result = _sut.Error(404);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("PageNotFound");
            _mockResponse.VerifySet(r => r.StatusCode = StatusCodes.Status404NotFound);
        }

        [Test]
        public void Error_Should_Return_Default_ViewModel_With_StatusCode_500_When_Id_Is_Null()
        {
            // Arrange
            var features = new FeatureCollection();
            _mockHttpContext.Setup(h => h.Features).Returns(features);
            _mockHttpContext.SetupGet(h => h.TraceIdentifier).Returns("trace-123");

            // Act
            var result = _sut.Error(null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
            model.RequestId.Should().Be("trace-123");
            _mockResponse.VerifySet(r => r.StatusCode = StatusCodes.Status500InternalServerError);
        }

        [Test]
        public void Error_Should_Set_StatusCode_To_Provided_Id_When_Not_404()
        {
            // Arrange
            var features = new FeatureCollection();
            _mockHttpContext.Setup(h => h.Features).Returns(features);
            _mockHttpContext.SetupGet(h => h.TraceIdentifier).Returns("trace-456");

            // Act
            var result = _sut.Error(400);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<ErrorViewModel>().Subject;
            model.RequestId.Should().Be("trace-456");
            _mockResponse.VerifySet(r => r.StatusCode = 400);
        }

        [Test]
        public void Error_Action_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ErrorController)
                .GetMethod(nameof(ErrorController.Error));
            var routeAttr = (RouteAttribute)Attribute.GetCustomAttribute(method, typeof(RouteAttribute));

            // Assert
            routeAttr.Should().NotBeNull();
            routeAttr.Name.Should().Be(ErrorController.ErrorGet);
        }
    }
}
