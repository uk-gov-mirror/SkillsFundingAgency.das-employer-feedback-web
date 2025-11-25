using System.Net;
using FluentAssertions;
using Moq;
using RestEase;
using SFA.DAS.EmployerFeedback.Application.Queries.GetAllQuestionAttributes;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Application.Queries
{
    [TestFixture]
    public class GetAllQuestionAttributesQueryHandlerTests
    {
        private Mock<IEmployerFeedbackOuterApi> _mockOuterApi;
        private GetAllQuestionAttributesQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _mockOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _sut = new GetAllQuestionAttributesQueryHandler(_mockOuterApi.Object);
        }

        [Test]
        public async Task Handle_Should_Return_QuestionAttributes_When_Successful()
        {
            // Arrange
            var expectedAttributes = new List<QuestionAttribute>
            {
                new QuestionAttribute { AttributeId = 1, AttributeName = "Helpful" },
                new QuestionAttribute { AttributeId = 2, AttributeName = "Friendly" }
            };

            _mockOuterApi
                .Setup(x => x.GetAllQuestionAttributes())
                .ReturnsAsync(expectedAttributes);

            var query = new GetAllQuestionAttributesQuery();

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(expectedAttributes);
            _mockOuterApi.Verify(x => x.GetAllQuestionAttributes(), Times.Once);
        }

        [Test]
        public void Handle_Should_Throw_InvalidOperationException_When_ApiException_Occurs()
        {
            // Arrange
            var query = new GetAllQuestionAttributesQuery();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com");
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                ReasonPhrase = "Internal Server Error"
            };
            response.Headers.TryAddWithoutValidation("X-Test", "HeaderValue");

            var apiException = new ApiException(
                request.Method,
                request.RequestUri,
                response.StatusCode,
                response.ReasonPhrase,
                response.Headers,
                response.Content?.Headers,
                "Server error content");

            _mockOuterApi
                .Setup(x => x.GetAllQuestionAttributes())
                .ThrowsAsync(apiException);

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("The question attributes cannot be retrieved");
        }
    }
}
