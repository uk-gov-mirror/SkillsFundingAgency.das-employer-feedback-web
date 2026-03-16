using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Infrastructure.UnitTests.Services.SessionStorage
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<ISession> _sessionMock;
        private Mock<HttpContext> _httpContextMock;
        private SessionStorageService _sessionStorageService;

        [SetUp]
        public void Setup()
        {
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionMock = new Mock<ISession>();
            _httpContextMock = new Mock<HttpContext>();

            _httpContextMock.Setup(x => x.Session).Returns(_sessionMock.Object);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);

            _sessionStorageService = new SessionStorageService(_httpContextAccessorMock.Object);
        }

        [Test]
        public void  Set_ShouldStoreItemInSession()
        {
            // Arrange
            var key = "testKey";
            var value = "testValue";

            // Act
             _sessionStorageService.Set(key, value);

            // Assert
            _sessionMock.Verify(x => x.Set(
                key,
                It.Is<byte[]>(v => System.Text.Encoding.UTF8.GetString(v) == value)),
                Times.Once);
        }

        [Test]
        public void Get_ShouldReturnItemFromSession()
        {
            // Arrange
            var key = "testKey";
            var expectedValue = "testValue";
            var bytes = System.Text.Encoding.UTF8.GetBytes(expectedValue);

            _sessionMock
                .Setup(x => x.TryGetValue(key, out It.Ref<byte[]>.IsAny))
                .Callback(new SessionOutCallback((string _, out byte[] value) =>
                {
                    value = bytes;
                }))
                .Returns(true);

            // Act
            var result =  _sessionStorageService.Get(key);

            // Assert
            result.Should().Be(expectedValue);
        }

        [Test]
        public void Get_ShouldReturnNullWhenItemNotInSession()
        {
            // Arrange
            var key = "testKey";
            byte[] value = null;

            _sessionMock
                .Setup(x => x.TryGetValue(key, out value))
                .Returns(false);

            // Act
            var result = _sessionStorageService.Get(key);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Clear_ShouldRemoveItemFromSession()
        {
            // Arrange
            var key = "testKey";

            // Act
            _sessionStorageService.Clear(key);

            // Assert
            _sessionMock.Verify(x => x.Remove(key), Times.Once);
        }

        private delegate void SessionOutCallback(string key, out byte[] value);
    }
}