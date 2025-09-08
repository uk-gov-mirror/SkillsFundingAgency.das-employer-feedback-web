using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Application.Commands;

namespace UnitTests.Api
{
    public class FeedbackResultAnnualQueryTests
    {
        private Mock<ILogger<FeedbackResultAnnualQueryHandler>> mockLogger;
        private FeedbackResultAnnualQueryHandler handler;

        [SetUp]
        public void Setup()
        {
            mockLogger = new Mock<ILogger<FeedbackResultAnnualQueryHandler>>();
            handler = new FeedbackResultAnnualQueryHandler(mockLogger.Object);
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfNullReturnsEmptyCollection()
        {
            //FIXME - replace with outer API call (mock)

            //// Arrange
            //mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(123))
            //    .ReturnsAsync((IEnumerable<EmployerFeedbackResultSummary>)null);

            //// Act
            //var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 123 }, CancellationToken.None);

            //// Assert
            //response.Should().NotBeNull();
            //response.AnnualEmployerFeedbackDetails.Should().BeEmpty();

            throw new NotImplementedException();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfNoFeedbackReturnsEmptyCollection()
        {
            //FIXME - replace with outer API call (mock)
            //// Arrange
            //mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(456))
            //    .ReturnsAsync(new List<EmployerFeedbackResultSummary>());

            //// Act
            //var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 456 }, CancellationToken.None);

            //// Assert
            //response.Should().NotBeNull();
            //response.AnnualEmployerFeedbackDetails.Should().BeEmpty();

            throw new NotImplementedException();
        }

        [Test]
        public async Task WhenQueryingFeedbackResultAnnual_IfFeedbackExistsReturnsConvertedModel()
        {
            //FIXME - replace with outer API call (mock)
            //// Arrange
            //var summaries = new List<EmployerFeedbackResultSummary>
            //{
            //    new EmployerFeedbackResultSummary
            //    {
            //        Ukprn = 789,
            //        TimePeriod = "All",
            //        Stars = 4,
            //        ReviewCount = 10,
            //        AttributeName = "Providing the right training at the right time",
            //        Strength = 2,
            //        Weakness = 4
            //    },
            //    new EmployerFeedbackResultSummary
            //    {
            //        Ukprn = 789,
            //        TimePeriod = "All",
            //        Stars = 4,
            //        ReviewCount = 10,
            //        AttributeName = "Communication with employers",
            //        Strength = 2,
            //        Weakness = 4
            //    }
            //};

            //mockRepository.Setup(s => s.GetFeedbackResultSummaryAnnual(789))
            //    .ReturnsAsync(summaries);

            //// Act
            //var response = await handler.Handle(new FeedbackResultAnnualQuery { Ukprn = 789 }, CancellationToken.None);

            //// Assert
            //response.Should().NotBeNull();
            //response.AnnualEmployerFeedbackDetails.Should().NotBeEmpty();

            //var details = response.AnnualEmployerFeedbackDetails.First();
            //details.Ukprn.Should().Be(789);
            //details.TimePeriod.Should().Be("All");
            //details.Stars.Should().Be(4);
            //details.ReviewCount.Should().Be(10);
            //details.ProviderAttribute.Should().HaveCount(2);
            throw new NotImplementedException();
        }
    }
}
