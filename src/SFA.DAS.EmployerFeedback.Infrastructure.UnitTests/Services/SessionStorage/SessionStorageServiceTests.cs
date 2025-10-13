using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
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
            byte[] storedBytes = null;

            _distributedCacheMock
                .Setup(x => x.SetAsync(
                    EnvironmentName + "_" + key,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((k, b, o, t) => storedBytes = b)
                .Returns(Task.CompletedTask);

            // Act
            await _service.Set(key, value);

            // Assert
            _distributedCacheMock.Verify(x => x.SetAsync(
                EnvironmentName + "_" + key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);

            var json = System.Text.Encoding.UTF8.GetString(storedBytes);
            json.Should().Contain("Test");
            json.Should().Contain("123");
        }

        [Test]
        public async Task GetAsync_ShouldReturnDeserializedObject_WhenKeyExists()
        {
            // Arrange
            var key = "test-key";
            var expected = new TestObject { Name = "Test", Value = 123 };
            var json = JsonConvert.SerializeObject(expected);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            _distributedCacheMock
                .Setup(x => x.GetAsync(EnvironmentName + "_" + key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _service.Get<TestObject>(key);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(expected.Name);
            result.Value.Should().Be(expected.Value);
        }

        [Test]
        public async Task GetAsync_ShouldReturnDefault_WhenKeyDoesNotExist()
        {
            // Arrange
            var key = "missing-key";
            _distributedCacheMock
                .Setup(x => x.GetAsync(EnvironmentName + "_" + key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            // Act
            var result = await _service.Get<TestObject>(key);

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

        private class TestObject
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}