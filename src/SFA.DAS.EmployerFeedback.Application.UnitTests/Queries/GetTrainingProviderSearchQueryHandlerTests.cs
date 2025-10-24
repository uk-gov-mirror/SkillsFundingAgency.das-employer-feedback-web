using System.Net;
using FluentAssertions;
using Moq;
using RestEase;
using SFA.DAS.EmployerFeedback.Application.Queries.GetTrainingProviderSearch;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Application.Queries
{
    [TestFixture]
    public class GetTrainingProviderSearchQueryHandlerTests
    {
        private Mock<IEmployerFeedbackOuterApi> _mockOuterApi;
        private GetTrainingProviderSearchQueryHandler _sut;

        [SetUp]
        public void Setup()
        {
            _mockOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            _sut = new GetTrainingProviderSearchQueryHandler(_mockOuterApi.Object);
        }

        [Test]
        public async Task Handle_Should_Return_TrainingProviderSearchResponse_When_Successful()
        {
            // Arrange
            var query = new GetTrainingProviderSearchQuery
            {
                AccountId = 123,
                UserRef = Guid.NewGuid()
            };

            var expectedResponse = new TrainingProviderSearchResponse
            {
                Providers = new System.Collections.Generic.List<SFA.DAS.EmployerFeedback.Infrastructure.Api.Types.ProviderFeedback>()
            };

            _mockOuterApi
                .Setup(x => x.GetTrainingProviderSearch(query.AccountId, query.UserRef))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            _mockOuterApi.Verify(x => x.GetTrainingProviderSearch(query.AccountId, query.UserRef), Times.Once);
            result.Should().BeSameAs(expectedResponse);
        }

        [Test]
        public void Handle_Should_Throw_InvalidOperationException_When_ApiException_Occurs()
        {
            // Arrange
            var query = new GetTrainingProviderSearchQuery
            {
                AccountId = 456,
                UserRef = Guid.NewGuid()
            };

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
                .Setup(x => x.GetTrainingProviderSearch(query.AccountId, query.UserRef))
                .ThrowsAsync(apiException);

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("The training provider search cannot be retrieved");
        }
    }
}
