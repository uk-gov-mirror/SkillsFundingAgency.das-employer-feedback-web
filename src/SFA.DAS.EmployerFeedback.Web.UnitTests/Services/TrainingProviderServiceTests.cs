using Castle.Core.Logging;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.Encoding;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.Services
{
    public class TrainingProviderServiceTests
    {
        /*private readonly Mock<IEmployerFeedbackOuterApi> _employerFeedbackOuterApiMock;
        private readonly Mock<ILogger<TrainingProviderService>> _logger;
        private readonly EmployerFeedbackWebConfiguration _config;
        {
            FeedbackWaitPeriodDays = 21,
        };

        public class GetTrainingProviderSearchViewModel
        {
            public static IEnumerable<object[]> MultiplePagedProvidersTestData()
            {
                IEnumerable<ProviderFeedback> providers = new[]
                {
                    new ProviderFeedback { Ukprn = 1, ProviderName = "A" },
                    new ProviderFeedback { Ukprn = 2, ProviderName = "B" },
                    new ProviderFeedback { Ukprn = 3, ProviderName = "C" },
                    new ProviderFeedback { Ukprn = 4, ProviderName = "D" },
                    new ProviderFeedback { Ukprn = 5, ProviderName = "E" },
                    new ProviderFeedback { Ukprn = 6, ProviderName = "F" },
                    new ProviderFeedback { Ukprn = 7, ProviderName = "G" },
                    new ProviderFeedback { Ukprn = 8, ProviderName = "H" },
                    new ProviderFeedback { Ukprn = 9, ProviderName = "I" },
                    new ProviderFeedback { Ukprn = 10, ProviderName = "J" },
                    new ProviderFeedback { Ukprn = 11, ProviderName = "K" },
                };

                yield return new object[] { providers, 10, 1, "All", "All", "ProviderName", "Asc", 11, 2 };
            }

            [TestCaseSource(nameof(MultiplePagedProvidersTestData))]
            public async Task When_Providers_Exist_Then_Return_PagedResult(
                IEnumerable<ProviderFeedback> providers,
                int pageSize, int pageIndex, string selectedProviderName, string selectedFeedbackStatus,
                string sortColumn, string sortDirection, int expectedTotalRecordCount, int expectedTotalPages)
            {
                // Arrange
                var testAccountId = 1;
                var testAccountIdEncoded = "MANYTEST1";
                var userRef = new System.Guid();
                _encodingServiceMock.Setup(m => m.Decode(testAccountIdEncoded, EncodingType.AccountId)).Returns(testAccountId);
                var _employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();
                _employerFeedbackOuterApiMock
                    .Setup(m => m.GetTrainingProviderSearch(testAccountId, userRef)).ReturnsAsync(new GetProviderFeedback() { AccountId = testAccountId, Providers = providers.ToList() });

                var sut = new TrainingProviderService(
                    _encodingServiceMock.Object,
                    _config,
                    _employerFeedbackOuterApiMock.Object);

                // Act
                var model = await sut.GetTrainingProviderSearchViewModel(testAccountId,
                    testAccountIdEncoded, userRef, selectedProviderName, selectedFeedbackStatus, pageSize, pageIndex, sortColumn, sortDirection);

                // Assert
                model.Providers.TotalRecordCount.Should().Be(expectedTotalRecordCount);
                model.Providers.TotalPages.Should().Be(expectedTotalPages);
            }
        }*/
    }
}
