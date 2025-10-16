using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.UnitTests.Services.SessionStorage
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IDistributedCache> _distributedCacheMock;
        private EmployerFeedbackWebConfiguration _configurationMock;
        private Mock<IWebHostEnvironment> _environmentMock;
        private SessionStorageService _service;
        private readonly string EnvironmentName = "TEST";

        [SetUp]
        public void SetUp()
        {
            _distributedCacheMock = new Mock<IDistributedCache>();
            _configurationMock = new EmployerFeedbackWebConfiguration() { SlidingExpirationMinutes = 30 };
            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(x => x.EnvironmentName).Returns(EnvironmentName);
            _service = new SessionStorageService(_distributedCacheMock.Object, _configurationMock, _environmentMock.Object);
        }

        [Test]
        public async Task Set_ShouldSerializeAndStoreObject()
        {
            // Arrange
            var key = "test-key";
            var value = new { Name = "Test", Value = 123 };

            _distributedCacheMock
                .Setup(x => x.SetAsync(
                    EnvironmentName + "_" + key,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.Set(key, value);

            // Assert
            _distributedCacheMock.Verify(x => x.SetAsync(
                EnvironmentName + "_" + key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSurveyModel_ShouldReturnDeserializedObject_WhenKeyExists()
        {
            // Arrange
            var key = "test-key";
            var expected = new SurveyModel { AccountId = 123456, ProviderName = "Test Provider" };
            var json = JsonConvert.SerializeObject(expected);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            _distributedCacheMock
                .Setup(x => x.GetAsync(EnvironmentName + "_" + key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _service.GetSurveyModel(key);

            // Assert
            result.Should().NotBeNull();
            result.AccountId.Should().Be(expected.AccountId);
            result.ProviderName.Should().Be(expected.ProviderName);
        }

        [Test]
        public async Task GetSurveyModel_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "missing-key";
            _distributedCacheMock
                .Setup(x => x.GetAsync(EnvironmentName + "_" + key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            // Act
            var result = await _service.GetSurveyModel(key);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task RemoveAsync_ShouldCallRemoveOnCache()
        {
            // Arrange
            var key = "test-key";

            // Act
            await _service.Remove(key);

            // Assert
            _distributedCacheMock.Verify(x => x.RemoveAsync(EnvironmentName + "_" + key, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}