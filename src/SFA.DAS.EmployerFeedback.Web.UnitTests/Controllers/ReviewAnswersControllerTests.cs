using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.UserService;
using SFA.DAS.EmployerFeedback.Web.Controllers;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.ReviewAnswers.ReviewAnswers;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class ReviewAnswersControllerTests
    {
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<ReviewAnswersController>> _mockLogger;
        private Mock<IReviewAnswersOrchestrator> _mockOrchestrator;
        private ReviewAnswersController _sut;

        [SetUp]
        public void Setup()
        {
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<ReviewAnswersController>>();
            _mockOrchestrator = new Mock<IReviewAnswersOrchestrator>();

            _sut = new ReviewAnswersController(
                _mockUserService.Object,
                _mockLogger.Object,
                _mockOrchestrator.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public void ReviewAnswers_Get_Should_Return_View_With_ViewModel()
        {
            // Arrange
            var expectedViewModel = new ReviewAnswersViewModel();
            _mockOrchestrator.Setup(o => o.GetReviewAnswersViewModel()).Returns(expectedViewModel);

            // Act
            var result = _sut.ReviewAnswers();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public async Task ReviewAnswers_Post_Should_Redirect_To_FeedbackAlreadySubmitted_When_Cannot_Submit()
        {
            // Arrange
            var viewModel = new ReviewAnswersViewModel { EncodedAccountId = "EFG123" };
            _mockOrchestrator.Setup(o => o.CanSubmitFeedback()).ReturnsAsync(false);

            // Act
            var result = await _sut.ReviewAnswersConfirmed(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.CanSubmitFeedback(), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.FeedbackAlreadySubmittedGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG123");
        }

        [Test]
        public async Task ReviewAnswers_Post_Should_Redirect_To_ReviewAnswers_When_Submit_Fails()
        {
            // Arrange
            var viewModel = new ReviewAnswersViewModel { EncodedAccountId = "EFG234" };
            _mockOrchestrator.Setup(o => o.CanSubmitFeedback()).ReturnsAsync(true);
            _mockOrchestrator.Setup(o => o.SubmitEmployerFeedback(It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.ReviewAnswersConfirmed(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.CanSubmitFeedback(), Times.Once);
            _mockOrchestrator.Verify(o => o.SubmitEmployerFeedback(It.IsAny<ModelStateDictionary>()), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.ReviewAnswersGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG234");
        }

        [Test]
        public async Task ReviewAnswers_Post_Should_Redirect_To_FeedbackConfirmation_When_Submit_Succeeds()
        {
            // Arrange
            var viewModel = new ReviewAnswersViewModel { EncodedAccountId = "EFG345" };
            _mockOrchestrator.Setup(o => o.CanSubmitFeedback()).ReturnsAsync(true);
            _mockOrchestrator.Setup(o => o.SubmitEmployerFeedback(It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);

            // Act
            var result = await _sut.ReviewAnswersConfirmed(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.CanSubmitFeedback(), Times.Once);
            _mockOrchestrator.Verify(o => o.SubmitEmployerFeedback(It.IsAny<ModelStateDictionary>()), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.FeedbackConfirmationGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG345");
        }

        [Test]
        public void  FeedbackConfirmation_Should_Return_View_With_Model()
        {
            // Arrange
            var accountModel = new AccountModel { EncodedAccountId = "ABC123" };
            var expectedViewModel = new FeedbackConfirmationViewModel();
            _mockOrchestrator.Setup(o => o.GetFeedbackConfirmationViewModel(accountModel))
                .Returns(expectedViewModel);

            // Act
            var result = _sut.FeedbackConfirmation(accountModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public void FeedbackAlreadySubmitted_Should_Return_View_With_Model()
        {
            // Arrange
            var accountModel = new AccountModel { EncodedAccountId = "ABC456" };
            var expectedViewModel = new FeedbackAlreadySubmittedViewModel();
            _mockOrchestrator.Setup(o => o.GetFeedbackAlreadySubmittedViewModel(accountModel))
                .Returns(expectedViewModel);

            // Act
            var result = _sut.FeedbackAlreadySubmitted(accountModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public void ReviewAnswers_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ReviewAnswersController).GetMethod(nameof(ReviewAnswersController.ReviewAnswers));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ReviewAnswersController.ReviewAnswersGet);
        }

        [Test]
        public void ReviewAnswers_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ReviewAnswersController)
                .GetMethods()
                .Single(m => m.Name == nameof(ReviewAnswersController.ReviewAnswersConfirmed));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ReviewAnswersController.ReviewAnswersPost);
        }

        [Test]
        public void FeedbackConfirmation_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ReviewAnswersController).GetMethod(nameof(ReviewAnswersController.FeedbackConfirmation));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ReviewAnswersController.FeedbackConfirmationGet);
        }

        [Test]
        public void FeedbackAlreadySubmitted_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(ReviewAnswersController).GetMethod(nameof(ReviewAnswersController.FeedbackAlreadySubmitted));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(ReviewAnswersController.FeedbackAlreadySubmittedGet);
        }
    }
}
