using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.EmployerProvideFeedback.Controllers
{
    [TestFixture]
    public class ErrorControllerTests
    {
        private ErrorController _errorController;
        private Mock<ILogger<ErrorController>> _loggerMock;

        [SetUp]
        public void Arrange()
        {
            _loggerMock = new Mock<ILogger<ErrorController>>();
            _errorController = new ErrorController(_loggerMock.Object);
            _errorController.ControllerContext.HttpContext = new DefaultHttpContext();
        }

        [Test]
        public void Error_404_ReturnsPageNotFound()
        {
            var result = _errorController.Error(404);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            
            var viewResult = (ViewResult)result;
            viewResult.ViewName.Should().Be("PageNotFound");
        }

        [Test]
        public void Error_403_ReturnsView()
        {
            var result = _errorController.Error(403);
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;
            viewResult.ViewName.Should().BeNullOrEmpty();
        }


        [TearDown]
        public void CleanUp()
        {
            _errorController.Dispose();
        }
    }
}
