using System.Collections.Generic;
using System.Text.Json;
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

        [SetUp]
        public void Setup()
        {
            _sessionStorageServiceMock = new Mock<ISessionStorageService>();          
            _sessionService = new SessionService(_sessionStorageServiceMock.Object);           
        }

        private static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj);

        private static T FromJson<T>(string json) => JsonSerializer.Deserialize<T>(json)!;

        [Test]
        public  void SetSurveyModel_Should_Store_Serialized_SurveyModel()
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
             _sessionService.SetSurveyModel(survey);

            // Assert
            storedKey.Should().Be("SurveyModel");

            var savedItem = FromJson<SurveyModel>(storedValue);
            savedItem.AccountId.Should().Be(1);
            savedItem.ProviderName.Should().Be("Test Provider");
        }

        [Test]
        public void GetSurveyModel_Should_Return_Deserialized_SurveyModel()
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
            var result = _sessionService.GetSurveyModel();

            // Assert
            result.ProviderName.Should().Be("Stored");
        }

        [Test]
        public void  GetSurveyModel_Should_Return_null_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("SurveyModel"))
                .Returns((string?)null);

            // Act
            var result = _sessionService.GetSurveyModel();

            // Assert
            result.Should().BeNull();            
        }

        [Test]
        public void UpdateSurveyModel_Should_Apply_Action_And_Save()
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
            var result = _sessionService.UpdateSurveyModel(x => x.ProviderName = "Updated");

            // Assert
            result.ProviderName.Should().Be("Updated");
            savedItem.ProviderName.Should().Be("Updated");
        }

        [Test]
        public void GetPagingState_Should_Return_Deserialized_PagingState()
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
            var result = _sessionService.GetPagingState();

            // Assert
            result.PageIndex.Should().Be(2);
        }

        [Test]
        public void GetPagingState_Should_Return_Default_PagingState_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("PagingState"))
                .Returns((string?)null);

            // Act
            var result = _sessionService.GetPagingState();

            // Assert
            result.Should().BeEquivalentTo(new PagingState());
        }

        [Test]
        public void SetPagingState_Should_Store_Serialized_PagingState()
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
             _sessionService.SetPagingState(pagingState);

            // Assert
            storedKey.Should().Be("PagingState");
        }

        [Test]
        public void UpdatePagingState_Should_Create_If_Not_Exist_And_Apply_Action()
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
            var result = _sessionService.UpdatePagingState(x => x.PageIndex = 9);

            // Assert
            result.PageIndex.Should().Be(9);
            savedItem.PageIndex.Should().Be(9);
        }

        [Test]
        public void SetFeedbackSource_Should_Store_Serialized_FeedbackSource()
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
            _sessionService.SetFeedbackSource(feedbackSource);

            // Assert
            storedKey.Should().Be("FeedbackSource");
            savedValue.Should().Be(FeedbackSource.Email);
        }

        [Test]
        public void GetFeedbackSource_Should_Return_Deserialized_FeedbackSource()
        {
            // Arrange
            var expectedItem = FeedbackSource.AdHoc;

            _sessionStorageServiceMock
                .Setup(x => x.Get("FeedbackSource"))
                .Returns(ToJson(expectedItem));

            // Act
            var result = _sessionService.GetFeedbackSource();

            // Assert
            result.Should().Be(FeedbackSource.AdHoc);
        }

        [Test]
        public void GetFeedbackSource_Should_Return_Null_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("FeedbackSource"))
                .Returns((string?)null);

            // Act
            var result = _sessionService.GetFeedbackSource();

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void SetProviders_Should_Store_Serialized_Providers()
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
             _sessionService.SetProviders(providers);

            // Assert
            savedItem.Should().ContainSingle(x => x.ProviderName == "Provider A");
        }

        [Test]
        public void GetProviders_Should_Return_Deserialized_Providers()
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
            var result = _sessionService.GetProviders();

            // Assert
            result.Should().ContainSingle(x => x.ProviderName == "Provider X");
        }

        [Test]
        public void GetProviders_Should_Return_Empty_List_When_No_Value_Found()
        {
            // Arrange
            _sessionStorageServiceMock
                .Setup(x => x.Get("Providers"))
                .Returns((string?)null);

            // Act
            var result = _sessionService.GetProviders();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }


        [Test]
        public void  ClearUserSession_Should_Clear_All_User_Session_Keys()
        {
            // Act
            _sessionService.ClearUserSession();

            // Assert
            _sessionStorageServiceMock.Verify(x => x.Clear("SurveyModel"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("PagingState"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("FeedbackSource"), Times.Once);
            _sessionStorageServiceMock.Verify(x => x.Clear("Providers"), Times.Once);
            _sessionStorageServiceMock.VerifyNoOtherCalls();
        }
    }
}