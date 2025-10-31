using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Application.Queries.GetTrainingProviderSearch;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Types;
using SFA.DAS.EmployerFeedback.Infrastructure.Configuration;
using SFA.DAS.EmployerFeedback.Services;
using SFA.DAS.EmployerFeedback.Web.Models.Shared;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Services.TrainingProvider
{
    [TestFixture]
    public class TrainingProviderServiceTests
    {
        private Mock<IMediator> _mockMediator;
        private EmployerFeedbackWebConfiguration _config;
        private TrainingProviderService _sut;
        private Guid _userId;

        [SetUp]
        public void Setup()
        {
            _mockMediator = new Mock<IMediator>();
            _config = new EmployerFeedbackWebConfiguration
            {
                FeedbackWaitPeriodDays = 30
            };
            _sut = new TrainingProviderService(_mockMediator.Object, _config);
            _userId = Guid.NewGuid();
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Return_Filtered_And_Sorted_Providers()
        {
            // Arrange
            var accountId = 1L;
            var encodedAccountId = "ENC1";
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback
                {
                    Ukprn = 111,
                    ProviderName = "Zeta College",
                    Feedback = new Feedback
                    {
                        DateTimeCompleted = DateTime.UtcNow.AddDays(-40)
                    }
                },
                new ProviderFeedback
                {
                    Ukprn = 222,
                    ProviderName = "Alpha Academy",
                    Feedback = new Feedback
                    {
                        DateTimeCompleted = DateTime.UtcNow.AddDays(-40)
                    }
                }
            };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                encodedAccountId,
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.Default,
                sortOrder: SortOrder.Ascending);

            // Assert
            result.AccountId.Should().Be(accountId);
            result.EncodedAccountId.Should().Be(encodedAccountId);
            result.Providers.Should().NotBeNull();
            result.UnfilteredTotalRecordCount.Should().Be(2);
            result.ProviderNameFilter.Should().Contain(new[] { "Alpha Academy", "Zeta College" });
            result.FeedbackStatusFilter.Should().ContainSingle("Not yet submitted");

            // should be sorted by ProviderName ascending
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("Alpha Academy", "Zeta College");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Sort_By_FeedbackStatus_Ascending()
        {
            // Arrange
            var accountId = 10L;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback { Ukprn = 1, ProviderName = "A", Feedback = null },
                new ProviderFeedback { Ukprn = 2, ProviderName = "B", Feedback = new Feedback { DateTimeCompleted = DateTime.UtcNow.AddDays(-50) } }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.FeedbackStatus,
                sortOrder: SortOrder.Ascending);

            // Assert
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("A", "B"); // not yet submitted before Submitted
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Sort_By_FeedbackStatus_Descending()
        {
            // Arrange
            var accountId = 11L;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback { Ukprn = 1, ProviderName = "A", Feedback = null },
                new ProviderFeedback { Ukprn = 2, ProviderName = "B", Feedback = new Feedback { DateTimeCompleted = DateTime.UtcNow.AddDays(-50) } }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.FeedbackStatus,
                sortOrder: SortOrder.Descending);

            // Assert
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("B", "A"); // submitted before Not yet submitted
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Sort_By_DateSubmitted_Ascending()
        {
            // Arrange
            var accountId = 12L;
            var now = DateTime.UtcNow;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback { Ukprn = 1, ProviderName = "Old", Feedback = new Feedback { DateTimeCompleted = now.AddDays(-60) } },
                new ProviderFeedback { Ukprn = 2, ProviderName = "Recent", Feedback = new Feedback { DateTimeCompleted = now.AddDays(-10) } }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.DateSubmitted,
                sortOrder: SortOrder.Ascending);

            // Assert
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("Old", "Recent");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Sort_By_DateSubmitted_Descending()
        {
            // Arrange
            var accountId = 13L;
            var now = DateTime.UtcNow;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback { Ukprn = 1, ProviderName = "Old", Feedback = new Feedback { DateTimeCompleted = now.AddDays(-60) } },
                new ProviderFeedback { Ukprn = 2, ProviderName = "Recent", Feedback = new Feedback { DateTimeCompleted = now.AddDays(-10) } }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.DateSubmitted,
                sortOrder: SortOrder.Descending);

            // Assert
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("Recent", "Old");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Sort_By_ProviderName_Descending()
        {
            // Arrange
            var accountId = 14L;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback { Ukprn = 1, ProviderName = "Alpha", Feedback = new Feedback() },
                new ProviderFeedback { Ukprn = 2, ProviderName = "Zulu", Feedback = new Feedback() }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.Default,
                sortOrder: SortOrder.Descending);

            // Assert
            var names = result.Providers.Items.Select(p => p.ProviderName).ToList();
            names.Should().ContainInOrder("Zulu", "Alpha");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Filter_By_ProviderName()
        {
            // Arrange
            var accountId = 2L;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback
                {
                    Ukprn = 1, ProviderName = "Match", Feedback = new Feedback()
                },
                new ProviderFeedback
                {
                    Ukprn = 2, ProviderName = "Other", Feedback = new Feedback()
                }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: "Match",
                selectedFeedbackStatus: "",
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.Default,
                sortOrder: SortOrder.Ascending);

            // Assert
            result.Providers.Items.Should().ContainSingle(p => p.ProviderName == "Match");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Filter_By_FeedbackStatus_NotYetSubmitted()
        {
            // Arrange
            var accountId = 3L;
            var providers = new List<ProviderFeedback>
            {
                new ProviderFeedback
                {
                    Ukprn = 1,
                    ProviderName = "Submitted",
                    Feedback = new Feedback
                    {
                        DateTimeCompleted = DateTime.UtcNow.AddDays(-50)
                    }
                },
                new ProviderFeedback
                {
                    Ukprn = 2,
                    ProviderName = "Pending",
                    Feedback = null
                }
            };
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: "Not yet submitted",
                pageSize: 10,
                pageIndex: 1,
                sortColumn: SortColumn.Default,
                sortOrder: SortOrder.Ascending);

            // Assert
            result.Providers.Items.Should().ContainSingle(p => p.ProviderName == "Pending");
        }

        [Test]
        public async Task GetTrainingProviderSearchViewModel_Should_Paginate_Results()
        {
            // Arrange
            var accountId = 4L;
            var providers = Enumerable.Range(1, 20).Select(i =>
                new ProviderFeedback
                {
                    Ukprn = i,
                    ProviderName = $"Provider {i}",
                    Feedback = new Feedback()
                }).ToList();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TrainingProviderSearchResponse { Providers = providers });

            // Act
            var result = await _sut.GetTrainingProviderSearchViewModel(
                accountId,
                "ENC123",
                _userId,
                selectedProviderName: string.Empty,
                selectedFeedbackStatus: string.Empty,
                pageSize: 5,
                pageIndex: 2,
                sortColumn: SortColumn.Default,
                sortOrder: SortOrder.Ascending);

            // Assert
            result.Providers.Items.Should().HaveCount(5);
            result.Providers.PageIndex.Should().Be(2);
            result.Providers.TotalRecordCount.Should().Be(20);
        }

        [Test]
        public async Task CanSubmitFeedback_Should_Return_False_When_Feedback_Too_Recent()
        {
            // Arrange
            var survey = new SurveyModel { AccountId = 1, Ukprn = 999 };
            var recentDate = DateTime.UtcNow.AddDays(-5);
            var result = new TrainingProviderSearchResponse
            {
                Providers = new List<ProviderFeedback>
                {
                    new ProviderFeedback
                    {
                        Ukprn = 999,
                        Feedback = new Feedback { DateTimeCompleted = recentDate }
                    }
                }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var canSubmit = await _sut.CanSubmitFeedback(survey, _userId);

            // Assert
            canSubmit.Should().BeFalse();
        }

        [Test]
        public async Task CanSubmitFeedback_Should_Return_True_When_Feedback_Older_Than_WaitPeriod()
        {
            // Arrange
            var survey = new SurveyModel { AccountId = 1, Ukprn = 999 };
            var oldDate = DateTime.UtcNow.AddDays(-40);
            var result = new TrainingProviderSearchResponse
            {
                Providers = new List<ProviderFeedback>
                {
                    new ProviderFeedback
                    {
                        Ukprn = 999,
                        Feedback = new Feedback { DateTimeCompleted = oldDate }
                    }
                }
            };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetTrainingProviderSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            // Act
            var canSubmit = await _sut.CanSubmitFeedback(survey, _userId);

            // Assert
            canSubmit.Should().BeTrue();
        }
    }
}
