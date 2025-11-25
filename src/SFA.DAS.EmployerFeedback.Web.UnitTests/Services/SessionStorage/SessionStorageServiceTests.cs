using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Services
{
    [TestFixture]
    public class SessionStorageServiceTests
    {
        private Mock<IDistributedCache> _mockCache;
        private Mock<IWebHostEnvironment> _mockEnvironment;
        private EmployerFeedbackWebConfiguration _config;
        private SessionStorageService _sut;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _mockCache = new Mock<IDistributedCache>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockEnvironment.Setup(e => e.EnvironmentName).Returns("LOCAL");
            _config = new EmployerFeedbackWebConfiguration { SlidingExpirationMinutes = 15 };
            _sut = new SessionStorageService(_mockCache.Object, _config, _mockEnvironment.Object);
            _userId = Guid.NewGuid();
        }

        private static byte[] ToBytes(object obj) => System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        private static T FromBytes<T>(byte[] bytes) => JsonConvert.DeserializeObject<T>(System.Text.Encoding.UTF8.GetString(bytes));

        [Test]
        public async Task SetSurveyModel_Should_Serialize_And_Store_With_Environment_Prefix()
        {
            // Arrange
            var survey = new SurveyModel { AccountId = 1, ProviderName = "Test Provider" };
            string storedKey = null;
            byte[] storedValue = null;
            _mockCache
                .Setup(c => c.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, value, _, _) =>
                {
                    storedKey = key;
                    storedValue = value;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetSurveyModel(_userId, survey);

            // Assert
            storedKey.Should().Be("LOCAL_" + _userId);
            var saved = FromBytes<SurveyModel>(storedValue);
            saved.ProviderName.Should().Be("Test Provider");
        }

        [Test]
        public async Task GetSurveyModel_Should_Deserialize_Stored_Value()
        {
            // Arrange
            var expected = new SurveyModel { ProviderName = "Stored" };
            var bytes = ToBytes(expected);
            _mockCache.Setup(c => c.GetAsync("LOCAL_" + _userId.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetSurveyModel(_userId);

            // Assert
            result.ProviderName.Should().Be("Stored");
        }

        [Test]
        public async Task UpdateSurveyModel_Should_Apply_Action_And_Save()
        {
            // Arrange
            var existing = new SurveyModel { ProviderName = "Old" };
            var bytes = ToBytes(existing);
            _mockCache.Setup(c => c.GetAsync("LOCAL_" + _userId.ToString(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            SurveyModel saved = null;
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, val, _, _) =>
                {
                    saved = FromBytes<SurveyModel>(val);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdateSurveyModel(_userId, s => s.ProviderName = "Updated");

            // Assert
            saved.ProviderName.Should().Be("Updated");
        }

        [Test]
        public async Task GetPagingState_Should_Return_Deserialized_Value()
        {
            // Arrange
            var pagingState = new PagingState { PageIndex = 2 };
            var bytes = ToBytes(pagingState);
            _mockCache.Setup(c => c.GetAsync("LOCAL_" + _userId + "_PagingState", It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetPagingState(_userId);

            // Assert
            result.PageIndex.Should().Be(2);
        }

        [Test]
        public async Task SetPagingState_Should_Store_With_Environment_Prefix()
        {
            // Arrange
            var pagingState = new PagingState { PageIndex = 3 };
            string storedKey = null;
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, _, _, _) =>
                {
                    storedKey = key;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetPagingState(_userId, pagingState);

            // Assert
            storedKey.Should().Be("LOCAL_" + _userId + "_PagingState");
        }

        [Test]
        public async Task UpdatePagingState_Should_Create_If_Not_Exist_And_Apply_Action()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync("LOCAL_" + _userId + "_PagingState", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[])null);

            PagingState saved = null;
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, val, _, _) =>
                {
                    saved = FromBytes<PagingState>(val);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.UpdatePagingState(_userId, p => p.PageIndex = 9);

            // Assert
            saved.PageIndex.Should().Be(9);
        }

        [Test]
        public async Task SetFeedbackSource_Should_Save_To_Cache()
        {
            // Arrange
            var feedbackSource = FeedbackSource.Email;
            string storedKey = null;
            FeedbackSource savedValue = default;
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((key, val, _, _) =>
                {
                    storedKey = key;
                    savedValue = FromBytes<FeedbackSource>(val);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetFeedbackSource(_userId, feedbackSource);

            // Assert
            storedKey.Should().Be($"LOCAL_{_userId}_FeedbackSource");
            savedValue.Should().Be(FeedbackSource.Email);
        }

        [Test]
        public async Task GetFeedbackSource_Should_Deserialize_From_Cache()
        {
            // Arrange
            var expected = FeedbackSource.AdHoc;
            var bytes = ToBytes(expected);
            _mockCache.Setup(c => c.GetAsync($"LOCAL_{_userId}_FeedbackSource", It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetFeedbackSource(_userId);

            // Assert
            result.Should().Be(FeedbackSource.AdHoc);
        }

        [Test]
        public async Task SetProviders_Should_Serialize_And_Save()
        {
            // Arrange
            var providers = new List<ProviderSearchViewModel.EmployerTrainingProvider>
            {
                new ProviderSearchViewModel.EmployerTrainingProvider { ProviderName = "Provider A" }
            };
            List<ProviderSearchViewModel.EmployerTrainingProvider> saved = null;
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, val, _, _) =>
                {
                    saved = FromBytes<List<ProviderSearchViewModel.EmployerTrainingProvider>>(val);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sut.SetProviders(_userId, providers);

            // Assert
            saved.Should().ContainSingle(p => p.ProviderName == "Provider A");
        }

        [Test]
        public async Task GetProviders_Should_Return_Deserialized_List()
        {
            // Arrange
            var expected = new List<ProviderSearchViewModel.EmployerTrainingProvider>
            {
                new ProviderSearchViewModel.EmployerTrainingProvider { ProviderName = "Provider X" }
            };
            var bytes = ToBytes(expected);
            _mockCache.Setup(c => c.GetAsync($"LOCAL_{_userId}_Providers", It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytes);

            // Act
            var result = await _sut.GetProviders(_userId);

            // Assert
            result.Should().ContainSingle(p => p.ProviderName == "Provider X");
        }
    }
}
