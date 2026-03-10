using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Services.SessionStorage;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;
using SFA.DAS.EmployerFeedback.Web.Paging;
using SFA.DAS.EmployerFeedback.Web.Services.SessionStorage;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Services
{
    [TestFixture]
    public class SessionServiceServiceTests
    {
        private Mock<ISessionStorageService> _sessionStorageServiceMock;
        private Mock<IMediator> _mediatorMock;
        private SessionService _sessionService;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _sessionStorageServiceMock = new Mock<ISessionStorageService>();
            _mediatorMock = new Mock<IMediator>();
            _sessionService = new SessionService(_sessionStorageServiceMock.Object, _mediatorMock.Object);
            _userId = Guid.NewGuid();
        }

        private static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj);

        private static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json)!;

        [Test]
        public async Task SetSurveyModel_Should_Store_Serialized_SurveyModel()
        {
            // Arrange
            var survey = new SurveyModel
            {
                AccountId = 1,
                ProviderName = "Test Provider"
            };

            string storedKey = null!;
            string storedValue = null!;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, value) =>
                {
                    storedKey = key;
                    storedValue = value;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sessionService.SetSurveyModel(_userId, survey);

            // Assert
            storedKey.Should().Be(_userId.ToString());

            var savedItem = FromJson<SurveyModel>(storedValue);
            savedItem.AccountId.Should().Be(1);
            savedItem.ProviderName.Should().Be("Test Provider");
        }

        [Test]
        public async Task GetSurveyModel_Should_Return_Deserialized_SurveyModel()
        {
            // Arrange
            var expectedItem = new SurveyModel
            {
                ProviderName = "Stored"
            };

            _sessionStorageServiceMock
                .Setup(x => x.GetAsync(_userId.ToString()))
                .ReturnsAsync(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetSurveyModel(_userId);

            // Assert
            result.ProviderName.Should().Be("Stored");
        }

        [Test]
        public async Task GetSurveyModel_Should_Return_Empty_Model_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.GetAsync(_userId.ToString()))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _sessionService.GetSurveyModel(_userId);

            // Assert
            result.Should().NotBeNull();
            result.ProviderName.Should().BeNull();
        }

        [Test]
        public async Task UpdateSurveyModel_Should_Apply_Action_And_Save()
        {
            // Arrange
            var existingItem = new SurveyModel
            {
                ProviderName = "Old"
            };

            _sessionStorageServiceMock
                .Setup(x => x.GetAsync(_userId.ToString()))
                .ReturnsAsync(ToJson(existingItem));

            SurveyModel savedItem = null!;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<SurveyModel>(value);
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sessionService.UpdateSurveyModel(_userId, x => x.ProviderName = "Updated");

            // Assert
            result.ProviderName.Should().Be("Updated");
            savedItem.ProviderName.Should().Be("Updated");
        }

        [Test]
        public async Task GetPagingState_Should_Return_Deserialized_PagingState()
        {
            // Arrange
            var expectedItem = new PagingState
            {
                PageIndex = 2
            };

            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_PagingState"))
                .ReturnsAsync(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetPagingState(_userId);

            // Assert
            result.PageIndex.Should().Be(2);
        }

        [Test]
        public async Task GetPagingState_Should_Return_Default_PagingState_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_PagingState"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _sessionService.GetPagingState(_userId);

            // Assert
            result.Should().BeEquivalentTo(new PagingState());
        }

        [Test]
        public async Task SetPagingState_Should_Store_Serialized_PagingState()
        {
            // Arrange
            var pagingState = new PagingState
            {
                PageIndex = 3
            };

            string storedKey = null!;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, _) =>
                {
                    storedKey = key;
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sessionService.SetPagingState(_userId, pagingState);

            // Assert
            storedKey.Should().Be($"{_userId}_PagingState");
        }

        [Test]
        public async Task UpdatePagingState_Should_Create_If_Not_Exist_And_Apply_Action()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_PagingState"))
                .ReturnsAsync((string?)null);

            PagingState savedItem = null!;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<PagingState>(value);
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sessionService.UpdatePagingState(_userId, x => x.PageIndex = 9);

            // Assert
            result.PageIndex.Should().Be(9);
            savedItem.PageIndex.Should().Be(9);
        }

        [Test]
        public async Task SetFeedbackSource_Should_Store_Serialized_FeedbackSource()
        {
            // Arrange
            var feedbackSource = FeedbackSource.Email;
            string storedKey = null!;
            FeedbackSource savedValue = default;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, value) =>
                {
                    storedKey = key;
                    savedValue = FromJson<FeedbackSource>(value);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sessionService.SetFeedbackSource(_userId, feedbackSource);

            // Assert
            storedKey.Should().Be($"{_userId}_FeedbackSource");
            savedValue.Should().Be(FeedbackSource.Email);
        }

        [Test]
        public async Task GetFeedbackSource_Should_Return_Deserialized_FeedbackSource()
        {
            // Arrange
            var expectedItem = FeedbackSource.AdHoc;

            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_FeedbackSource"))
                .ReturnsAsync(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetFeedbackSource(_userId);

            // Assert
            result.Should().Be(FeedbackSource.AdHoc);
        }

        [Test]
        public async Task GetFeedbackSource_Should_Return_Default_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_FeedbackSource"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _sessionService.GetFeedbackSource(_userId);

            // Assert
            result.Should().Be(default(FeedbackSource));
        }

        [Test]
        public async Task SetProviders_Should_Store_Serialized_Providers()
        {
            // Arrange
            var providers = new List<ProviderSearchViewModel.EmployerTrainingProvider>
            {
                new ProviderSearchViewModel.EmployerTrainingProvider
                {
                    ProviderName = "Provider A"
                }
            };

            List<ProviderSearchViewModel.EmployerTrainingProvider> savedItem = null!;

            _sessionStorageServiceMock
                .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<List<ProviderSearchViewModel.EmployerTrainingProvider>>(value);
                })
                .Returns(Task.CompletedTask);

            // Act
            await _sessionService.SetProviders(_userId, providers);

            // Assert
            savedItem.Should().ContainSingle(x => x.ProviderName == "Provider A");
        }

        [Test]
        public async Task GetProviders_Should_Return_Deserialized_Providers()
        {
            // Arrange
            var expectedItem = new List<ProviderSearchViewModel.EmployerTrainingProvider>
            {
                new ProviderSearchViewModel.EmployerTrainingProvider
                {
                    ProviderName = "Provider X"
                }
            };

            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_Providers"))
                .ReturnsAsync(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetProviders(_userId);

            // Assert
            result.Should().ContainSingle(x => x.ProviderName == "Provider X");
        }

        [Test]
        public async Task GetProviders_Should_Return_Empty_List_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.GetAsync($"{_userId}_Providers"))
                .ReturnsAsync((string?)null);

            // Act
            var result = await _sessionService.GetProviders(_userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task ClearUserSession_Should_Remove_Only_SurveyModel_Key()
        {
            // Act
            await _sessionService.ClearUserSession(_userId);

            // Assert
            _sessionStorageServiceMock.Verify(x => x.ClearAsync(_userId.ToString()), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.ClearAsync($"{_userId}_PagingState"), Times.Never);
            _sessionStorageServiceMock.Verify(x => x.ClearAsync($"{_userId}_FeedbackSource"), Times.Never);
            _sessionStorageServiceMock.Verify(x => x.ClearAsync($"{_userId}_Providers"), Times.Never);
        }
    }
}