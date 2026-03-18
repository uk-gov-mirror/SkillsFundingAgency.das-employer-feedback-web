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
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Orchestrators;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Controllers
{
    [TestFixture]
    public class QuestionsControllerTests
    {
        private Mock<ISessionService> _mockSessionService;
        private Mock<IUserService> _mockUserService;
        private Mock<ILogger<QuestionsController>> _mockLogger;
        private Mock<IQuestionsOrchestrator> _mockOrchestrator;
        private QuestionsController _sut;

        [SetUp]
        public void Setup()
        {
            _mockSessionService = new Mock<ISessionService>();
            _mockUserService = new Mock<IUserService>();
            _mockLogger = new Mock<ILogger<QuestionsController>>();
            _mockOrchestrator = new Mock<IQuestionsOrchestrator>();

            _sut = new QuestionsController(
                _mockSessionService.Object,
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
        public void StartFeedback_Should_Return_View_With_Model()
        {
            // Arrange
            var accountModel = new AccountModel { EncodedAccountId = "ACC123" };
            var expectedViewModel = new StartFeedbackViewModel();
            _mockOrchestrator.Setup(o => o.GetStartFeedbackViewModel(accountModel)).Returns(expectedViewModel);

            // Act
            var result = _sut.StartFeedback(accountModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public  void QuestionOne_Get_Should_Return_View_With_Model()
        {
            // Arrange
            var model = new QuestionRequestModel();
            var expectedViewModel = new QuestionOneStrengthsViewModel();
            _mockOrchestrator.Setup(o => o.GetQuestionOneStrengthsViewModel(model)).Returns(expectedViewModel);

            // Act
            var result = _sut.QuestionOne(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public async Task QuestionOne_Post_Should_Redirect_To_Get_When_Invalid()
        {
            // Arrange
            var viewModel = new QuestionOneStrengthsViewModel { EncodedAccountId = "AAA1111", ReturnToReviewAnswers = false };
            _mockOrchestrator.Setup(o => o.ValidateQuestionOneStrengthsViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(false);

            // Act
            var result = _sut.QuestionOne(viewModel);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.QuestionOneGet);
        }

        [Test]
        public async Task QuestionOne_Post_Should_Update_And_Redirect_To_Review_When_ReturnToReviewAnswers_True()
        {
            // Arrange
            var viewModel = new QuestionOneStrengthsViewModel { EncodedAccountId = "EFG123", ReturnToReviewAnswers = true };
            _mockOrchestrator.Setup(o => o.ValidateQuestionOneStrengthsViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(true);

            // Act
            var result = _sut.QuestionOne(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateQuestionOneAnswers(viewModel), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.ReviewAnswersGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG123");
        }

        [Test]
        public async Task QuestionOne_Post_Should_Update_And_Redirect_To_QuestionTwo_When_Valid()
        {
            // Arrange
            var viewModel = new QuestionOneStrengthsViewModel { EncodedAccountId = "MNO222", ReturnToReviewAnswers = false };
            _mockOrchestrator.Setup(o => o.ValidateQuestionOneStrengthsViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(true);

            // Act
            var result = _sut.QuestionOne(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateQuestionOneAnswers(viewModel), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.QuestionTwoGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("MNO222");
        }

        [Test]
        public async Task QuestionTwo_Get_Should_Return_View_With_Model()
        {

            // Arrange
            var model = new QuestionRequestModel();
            var expectedViewModel = new QuestionTwoWeaknessesViewModel();
            _mockOrchestrator.Setup(o => o.GetQuestionTwoWeaknessesViewModel(model)).Returns(expectedViewModel);

            // Act
            var result = _sut.QuestionTwo(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public async Task QuestionTwo_Post_Should_Redirect_To_Get_When_Invalid()
        {
            // Arrange
            var viewModel = new QuestionTwoWeaknessesViewModel { EncodedAccountId = "ABC321", ReturnToReviewAnswers = false };
            _mockOrchestrator.Setup(o => o.ValidateQuestionTwoWeaknessesViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(false);

            // Act
            var result = _sut.QuestionTwo(viewModel);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.QuestionTwoGet);
        }

        [Test]
        public async Task QuestionTwo_Post_Should_Update_And_Redirect_To_Review_When_ReturnToReviewAnswers_True()
        {
            // Arrange
            var viewModel = new QuestionTwoWeaknessesViewModel { EncodedAccountId = "CAB123", ReturnToReviewAnswers = true };
            _mockOrchestrator.Setup(o => o.ValidateQuestionTwoWeaknessesViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(true);

            // Act
            var result = _sut.QuestionTwo(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateQuestionTwoAnswers(viewModel), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.ReviewAnswersGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("CAB123");
        }

        [Test]
        public async Task QuestionTwo_Post_Should_Update_And_Redirect_To_QuestionThree_When_Valid()
        {
            // Arrange
            var viewModel = new QuestionTwoWeaknessesViewModel { EncodedAccountId = "EFG432", ReturnToReviewAnswers = false };
            _mockOrchestrator.Setup(o => o.ValidateQuestionTwoWeaknessesViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(true);

            // Act
            var result = _sut.QuestionTwo(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateQuestionTwoAnswers(viewModel), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.QuestionThreeGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("EFG432");
        }

        [Test]
        public async Task QuestionThree_Get_Should_Return_View_With_Model()
        {
            // Arrange
            var model = new QuestionRequestModel();
            var expectedViewModel = new QuestionThreeRatingViewModel();
            _mockOrchestrator.Setup(o => o.GetQuestionThreeRatingViewModel(model)).Returns(expectedViewModel);

            // Act
            var result = _sut.QuestionThree(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(expectedViewModel);
        }

        [Test]
        public async Task QuestionThree_Post_Should_Redirect_To_Get_When_Invalid()
        {
            // Arrange
            var viewModel = new QuestionThreeRatingViewModel { EncodedAccountId = "ABC321", ReturnToReviewAnswers = false };
            _mockOrchestrator.Setup(o => o.ValidateQuestionThreeRatingViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(false);

            // Act
            var result = _sut.QuestionThree(viewModel);

            // Assert
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(QuestionsController.QuestionThreeGet);
        }

        [Test]
        public async Task QuestionThree_Post_Should_Update_And_Redirect_To_ReviewAnswers_When_Valid()
        {
            // Arrange
            var viewModel = new QuestionThreeRatingViewModel { EncodedAccountId = "XYZ123" };
            _mockOrchestrator.Setup(o => o.ValidateQuestionThreeRatingViewModel(viewModel, It.IsAny<ModelStateDictionary>()))
                .Returns(true);

            // Act
            var result = _sut.QuestionThree(viewModel);

            // Assert
            _mockOrchestrator.Verify(o => o.UpdateQuestionThreeAnswers(viewModel), Times.Once);
            var redirect = result.Should().BeOfType<RedirectToRouteResult>().Subject;
            redirect.RouteName.Should().Be(ReviewAnswersController.ReviewAnswersGet);
            redirect.RouteValues["encodedAccountId"].Should().Be("XYZ123");
        }

        [Test]
        public void StartFeedback_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController).GetMethod(nameof(QuestionsController.StartFeedback));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.StartFeedbackGet);
        }

        [Test]
        public void QuestionOne_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionOne) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionRequestModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionOneGet);
        }

        [Test]
        public void QuestionOne_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionOne) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionOneStrengthsViewModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionOnePost);
        }

        [Test]
        public void QuestionTwo_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionTwo) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionRequestModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionTwoGet);
        }

        [Test]
        public void QuestionTwo_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionTwo) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionTwoWeaknessesViewModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionTwoPost);
        }

        [Test]
        public void QuestionThree_Get_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionThree) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionRequestModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionThreeGet);
        }

        [Test]
        public void QuestionThree_Post_Should_Have_Correct_Route_Name()
        {
            // Arrange
            var method = typeof(QuestionsController)
                .GetMethods()
                .Single(m => m.Name == nameof(QuestionsController.QuestionThree) &&
                             m.GetParameters()[0].ParameterType == typeof(QuestionThreeRatingViewModel));

            // Act
            var routeAttr = (RouteAttribute)method.GetCustomAttributes(typeof(RouteAttribute), false).Single();

            // Assert
            routeAttr.Name.Should().Be(QuestionsController.QuestionThreePost);
        }
    }
}
