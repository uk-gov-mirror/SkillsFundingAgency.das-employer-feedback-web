using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
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
    public class SessionStorageServiceTests
    {
        private Mock<ISessionStorageService> _sessionStorageServiceMock;       
        private SessionService _sessionService;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _sessionStorageServiceMock = new Mock<ISessionStorageService>();          
            _sessionService = new SessionService(_sessionStorageServiceMock.Object);
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
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, value) =>
                {
                    storedKey = key;
                    storedValue = value;
                });               

            // Act
            await _sessionService.SetSurveyModel(_userId, survey);

            // Assert
            storedKey.Should().Be("SurveyModel");

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
                .Setup(x => x.Get("SurveyModel"))
                .Returns(ToJson(expectedItem));

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
                .Setup(x => x.Get("SurveyModel"))
                .Returns((string?)null);

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
                .Setup(x => x.Get("SurveyModel"))
                .Returns(ToJson(existingItem));

            SurveyModel savedItem = null!;

            _sessionStorageServiceMock
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<SurveyModel>(value);
                });

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
                .Setup(x => x.Get("PagingState"))
                .Returns(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetPagingState();

            // Assert
            result.PageIndex.Should().Be(2);
        }

        [Test]
        public async Task GetPagingState_Should_Return_Default_PagingState_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("PagingState"))
                .Returns((string?)null);

            // Act
            var result = await _sessionService.GetPagingState();

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
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, _) =>
                {
                    storedKey = key;
                });
            
            // Act
            await _sessionService.SetPagingState(pagingState);

            // Assert
            storedKey.Should().Be("PagingState");
        }

        [Test]
        public async Task UpdatePagingState_Should_Create_If_Not_Exist_And_Apply_Action()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("PagingState"))
                .Returns((string?)null);

            PagingState savedItem = null!;

            _sessionStorageServiceMock
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<PagingState>(value);
                });               

            // Act
            var result = await _sessionService.UpdatePagingState(x => x.PageIndex = 9);

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
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((key, value) =>
                {
                    storedKey = key;
                    savedValue = FromJson<FeedbackSource>(value);
                });               

            // Act
            await _sessionService.SetFeedbackSource(_userId, feedbackSource);

            // Assert
            storedKey.Should().Be("FeedbackSource");
            savedValue.Should().Be(FeedbackSource.Email);
        }

        [Test]
        public async Task GetFeedbackSource_Should_Return_Deserialized_FeedbackSource()
        {
            // Arrange
            var expectedItem = FeedbackSource.AdHoc;

            _sessionStorageServiceMock
                .Setup(x => x.Get("FeedbackSource"))
                .Returns(ToJson(expectedItem));

            // Act
            var result = await _sessionService.GetFeedbackSource(_userId);

            // Assert
            result.Should().Be(FeedbackSource.AdHoc);
        }

        [Test]
        public async Task GetFeedbackSource_Should_Return_Null_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("FeedbackSource"))
                .Returns((string?)null);

            // Act
            var result = await _sessionService.GetFeedbackSource(_userId);

            // Assert
            result.Should().BeNull();
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
                .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((_, value) =>
                {
                    savedItem = FromJson<List<ProviderSearchViewModel.EmployerTrainingProvider>>(value);
                });             

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
                .Setup(x => x.Get("Providers"))
                .Returns(ToJson(expectedItem));

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
                .Setup(x => x.Get("Providers"))
                .Returns((string?)null);

            // Act
            var result = await _sessionService.GetProviders(_userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }


        [Test]
        public async Task ClearUserSession_Should_Clear_All_User_Session_Keys()
        {
            // Act
            await _sessionService.ClearUserSession(_userId);

            // Assert
            _sessionStorageServiceMock.Verify(x => x.Clear("SurveyModel"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("PagingState"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("FeedbackSource"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("Providers"), Times.Once);
            _sessionStorageServiceMock.VerifyNoOtherCalls();
        }
    }
}