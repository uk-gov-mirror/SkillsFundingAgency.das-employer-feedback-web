using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmployerFeedback.Application.Commands.SubmitEmployerFeedback;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Requests;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using SFA.DAS.EmployerFeedback.TestHelper.Extensions;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Application.Commands
{
    [TestFixture]
    public class SubmitEmployerFeedbackCommandHandlerTests
    {
        private Mock<IEmployerFeedbackOuterApi> _mockOuterApi;
        private Mock<ILogger<SubmitEmployerFeedbackCommandHandler>> _mockLogger;
        private SubmitEmployerFeedbackCommandHandler _sut;

        [SetUp]
        public void Setup()
        {
            _mockOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _mockLogger = new Mock<ILogger<SubmitEmployerFeedbackCommandHandler>>();
            _sut = new SubmitEmployerFeedbackCommandHandler(_mockOuterApi.Object, _mockLogger.Object);
        }

        [Test]
        public async Task Handle_Should_Return_True_When_Api_Succeeds()
        {
            // Arrange
            var command = new SubmitEmployerFeedbackCommand
            {
                Ukprn = 12345678,
                AccountId = 9876,
                Rating = "Good",
                FeedbackSource = FeedbackSource.AdHoc,
                UserRef = Guid.NewGuid(),
                Attributes = new List<ProviderAttribute>()
            };

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
            _mockOuterApi.Verify(x => x.SubmitEmployerFeedback(
                It.Is<SubmitEmployerFeedbackRequest>(r =>
                    r.Ukprn == command.Ukprn &&
                    r.AccountId == command.AccountId &&
                    r.ProviderRating == command.Rating &&
                    r.FeedbackSource == command.FeedbackSource &&
                    r.UserRef == command.UserRef &&
                    r.ProviderAttributes == command.Attributes)),
                Times.Once);
        }

        [Test]
        public async Task Handle_Should_Return_False_And_LogError_When_Api_Throws_Exception()
        {
            // Arrange
            var command = new SubmitEmployerFeedbackCommand
            {
                Ukprn = 22222222,
                AccountId = 1111,
                Rating = "Excellent",
                FeedbackSource = FeedbackSource.Email,
                UserRef = Guid.NewGuid(),
                Attributes = new System.Collections.Generic.List<ProviderAttribute>()
            };

            _mockOuterApi
                .Setup(x => x.SubmitEmployerFeedback(It.IsAny<SubmitEmployerFeedbackRequest>()))
                .ThrowsAsync(new InvalidOperationException("API failure"));

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
            _mockLogger.VerifyLogError("Error occurred when submitting feedback",
                Times.Once);
        }
    }
}
