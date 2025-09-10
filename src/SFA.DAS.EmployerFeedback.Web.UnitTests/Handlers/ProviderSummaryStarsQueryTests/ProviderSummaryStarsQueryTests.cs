using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Application.Commands;
using SFA.DAS.EmployerFeedback.Application.Queries;
using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

namespace UnitTests.Api
{
    public class ProviderSummaryStarsQueryTests
    {
        private readonly Mock<ILogger<ProviderSummaryStarsQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackOuterApi> employerFeedbackOuterApi;
        private readonly ProviderSummaryStarsQueryHandler handler;

        public ProviderSummaryStarsQueryTests()
        {
            mockLogger = new Mock<ILogger<ProviderSummaryStarsQueryHandler>>();
            employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            handler = new ProviderSummaryStarsQueryHandler(employerFeedbackOuterApi.Object, mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfNullReturnsNull()
        {
            // Arrange
            employerFeedbackOuterApi
                .Setup(s => s.GetAllStarsSummary("AY2024"))
                .ReturnsAsync((IEnumerable<ProviderStarsSummary>)null);

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery() { TimePeriod = "AY2024" }, new CancellationToken());

            // Assert
            response.Should().BeNull();
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfNoSummaryStarsReturnsEmptyCollection()
        {
            // Arrange
            employerFeedbackOuterApi
                .Setup(s => s.GetAllStarsSummary("AY2024"))
                .ReturnsAsync(new List<ProviderStarsSummary>());

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery() { TimePeriod = "AY2024" }, new CancellationToken());

            // Assert
            response.Should().BeAssignableTo<IEnumerable<EmployerFeedbackStarsSummary>>();
            response.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingProviderSummaryStars_IfSummaryStarsExistsReturnsConvertedModel()
        {
            // Arrange
            var summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(10).ToList();
            employerFeedbackOuterApi
                .Setup(s => s.GetAllStarsSummary("AY2024"))
                .ReturnsAsync(summaries);

            // Act
            var response = await handler.Handle(new ProviderSummaryStarsQuery(){TimePeriod = "AY2024"}, new CancellationToken());

            // Assert
            response.Should().BeAssignableTo<IEnumerable<EmployerFeedbackStarsSummary>>();
            response.Should().NotBeEmpty();

            response.Should().BeEquivalentTo(summaries.Select(s => new EmployerFeedbackStarsSummary
            {
                Ukprn = s.Ukprn,
                ReviewCount = s.ReviewCount,
                Stars = s.Stars,
            }));
        }
    }
}
