using System;
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
    public class FeedbackResultForAcademicYearQueryTests
    {
        private readonly Mock<ILogger<FeedbackResultForAcademicYearQueryHandler>> mockLogger;
        private readonly Mock<IEmployerFeedbackOuterApi> employerFeedbackOuterApiMock;
        private readonly FeedbackResultForAcademicYearQueryHandler handler;

        public FeedbackResultForAcademicYearQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackResultForAcademicYearQueryHandler>>();
            employerFeedbackOuterApiMock = new Mock<IEmployerFeedbackOuterApi>();
            handler = new FeedbackResultForAcademicYearQueryHandler(employerFeedbackOuterApiMock.Object, mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfNullReturnsEmptyCollection()
        {
            // Arrange
            employerFeedbackOuterApiMock
                .Setup(s => s.GetFeedbackResultSummaryForAcademicYear(123, "AY2324"))
                .ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            // Act
            var response = await handler.Handle(new FeedbackResultForAcademicYearQuery { Ukprn = 123 }, new CancellationToken());

            // Assert
            response.Ukprn.Should().Be(123);
            response.ReviewCount.Should().Be(0);
            response.Stars.Should().Be(0);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>();
            response.ProviderAttribute.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfNoFeedbackReturnsEmptyCollection()
        {

            // Arrange
            employerFeedbackOuterApiMock
                .Setup(s => s.GetFeedbackResultSummaryForAcademicYear(456, "AY2324"))
                .ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            // Act
            var response = await handler.Handle(new FeedbackResultForAcademicYearQuery { Ukprn = 456 }, new CancellationToken());

            // Assert
            response.Ukprn.Should().Be(456);
            response.ReviewCount.Should().Be(0);
            response.Stars.Should().Be(0);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>();
            response.ProviderAttribute.Should().BeEmpty();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultForAcademicYear_IfFeedbackExistsReturnsConvertedModel()
        {

            // Arrange
            var summaries = new Fixture().CreateMany<EmployerFeedbackResultSummary>(1).ToList();
            summaries[0].Ukprn = 789;

            employerFeedbackOuterApiMock
                .Setup(s => s.GetFeedbackResultSummaryForAcademicYear(789, "AY2324"))
                .ReturnsAsync(summaries);

            // Act
            var response = await handler.Handle(new FeedbackResultForAcademicYearQuery { Ukprn = 789, AcademicYear = "AY2324" }, new CancellationToken());

            // Assert
            var summary = summaries[0];
            response.Ukprn.Should().Be(summary.Ukprn);
            response.ReviewCount.Should().Be(summary.ReviewCount);
            response.Stars.Should().Be(summary.Stars);
            response.ProviderAttribute.Should().BeAssignableTo<IEnumerable<ProviderAttributeForAcademicYearSummaryItemDto>>();
            response.ProviderAttribute.Should().NotBeEmpty();

        }
    }
}
