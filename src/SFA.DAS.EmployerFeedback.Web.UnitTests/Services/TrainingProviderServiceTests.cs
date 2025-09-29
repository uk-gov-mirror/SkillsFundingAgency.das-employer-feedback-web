using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerProvideFeedback.Services;
using SFA.DAS.Encoding;

namespace UnitTests.Services
{
    public class TrainingProviderServiceTests
    {
        private static readonly Mock<IEncodingService> _encodingServiceMock = new();
        private static readonly Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApiMock = new();
        private static readonly EmployerFeedbackWebConfiguration _config = new()
        {
            FeedbackWaitPeriodDays = 21,
        };

        public class GetTrainingProviderSearchViewModel
        {
            public static IEnumerable<object[]> MultiplePagedProvidersTestData()
            {
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers = new[]
                {
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 1, ProviderName = "A" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 2, ProviderName = "B" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 3, ProviderName = "C" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 4, ProviderName = "D" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 5, ProviderName = "E" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 6, ProviderName = "F" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 7, ProviderName = "G" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 8, ProviderName = "H" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 9, ProviderName = "I" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 10, ProviderName = "J" },
                    new GetApprenticeshipsResponse.ApprenticeshipDetailsResponse { ProviderId = 11, ProviderName = "K" },
                };

                yield return new object[] { providers, 10, 1, "All", "All", "ProviderName", "Asc", 11, 2 };
            }

            [TestCaseSource(nameof(MultiplePagedProvidersTestData))]
            public async Task When_Providers_Exist_Then_Return_PagedResult(
                IEnumerable<GetApprenticeshipsResponse.ApprenticeshipDetailsResponse> providers,
                int pageSize, int pageIndex, string selectedProviderName, string selectedFeedbackStatus,
                string sortColumn, string sortDirection, int expectedTotalRecordCount, int expectedTotalPages)
            {
                // Arrange
                var testAccountId = 1;
                var testAccountIdEncoded = "MANYTEST1";
                var userRef = new System.Guid();
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);
                var _employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();

                ITrainingProviderService sut = new TrainingProviderService(
                    _encodingServiceMock.Object,
                    _config,
                    _employerFeedbackOuterApiMock.Object);

                // Act
                var model = await sut.GetTrainingProviderSearchViewModel(
                    testAccountIdEncoded, userRef, selectedProviderName, selectedFeedbackStatus, pageSize, pageIndex, sortColumn, sortDirection);

                // Assert
                model.TrainingProviders.TotalRecordCount.Should().Be(expectedTotalRecordCount);
                model.TrainingProviders.TotalPages.Should().Be(expectedTotalPages);
            }
        }

        public class GetTrainingProviderConfirmationViewModel
        {
            [Test]
            public async Task When_Provider_Exists_Then_Return_ProviderViewModel()
            {
                // Arrange
                var testAccountId = 2;
                var testUserRef = new System.Guid();
                var testAccountIdEncoded = "CONFIRMATIONMODELTEST1";
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);

                ITrainingProviderService sut = new TrainingProviderService(
                    _encodingServiceMock.Object,
                    _config,
                    _employerFeedbackOuterApiMock.Object);

                // Act
                var model = await sut.GetTrainingProviderConfirmationViewModel(testAccountId, testUserRef, 1);

                // Assert
                model.Should().NotBeNull();
                model.ProviderId.Should().Be(1);
                model.ProviderName.Should().Be("Test Provider");
            }
        }
    }
}
