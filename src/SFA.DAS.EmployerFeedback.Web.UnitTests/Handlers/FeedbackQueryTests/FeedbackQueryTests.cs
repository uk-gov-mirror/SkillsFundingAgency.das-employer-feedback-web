namespace UnitTests.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.NUnit3;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NUnit.Framework;
    using SFA.DAS.EmployerFeedback.Application.Commands;

    public class FeedbackQueryTests
    {
        private readonly FeedbackQueryHandler handler;

        private readonly Mock<ILogger<FeedbackQueryHandler>> mockLogger;

        public FeedbackQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackQueryHandler>>();
            handler = new FeedbackQueryHandler(mockLogger.Object);
        }


        [Test]
        public async Task WhenQueryingFeedback_IfNullReturnsEmptyCollection()
        {
            // FIXME  - Replace with outer API call
            //// Arrange
            //mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync((IEnumerable<EmployerFeedbackViewModel>) null);

            //// Act
            //var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            //// Assert
            //response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
            throw new NotImplementedException();
        }

        [Test]
        public async Task WhenQueryingFeedback_IfNoFeedbackReturnsEmptyCollection()
        {
            // FIXME  - Replace with outer API call
            //// Arrange
            //mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(new List<EmployerFeedbackViewModel>());

            //// Act
            //var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            //// Assert
            //response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
            throw new NotImplementedException();
        }

        [Test]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsConvertedModel()
        {
            // FIXME  - Replace with outer API call
            //// Arrange
            //var fixture = new Fixture();
            //var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(150);
            //mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            //// Act
            //var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            //// Assert
            //response.Should().BeEquivalentTo(feedback.Select(s => new EmployerFeedbackDto
            //{
            //    Ukprn = s.Ukprn,
            //    ProviderRating = s.ProviderRating,
            //    DateTimeCompleted = s.DateTimeCompleted,
            //    ProviderAttributes = new List<ProviderAttributeDto> { new ProviderAttributeDto { Name = s.AttributeName, Value = s.AttributeValue } }
            //}));
            throw new NotImplementedException();
        }

        [Test, AutoData]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsGroupedFeedback(Guid Id, long ukprn, string providerRating, DateTime dateTimeCompleted)
        {
            // FIXME  - Replace with outer API call
            //// Arrange
            //var fixture = new Fixture();
            //var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(10);
            //foreach(var f in feedback)
            //{
            //    f.Id = Id;
            //    f.Ukprn = ukprn;
            //    f.ProviderRating = providerRating;
            //    f.DateTimeCompleted = dateTimeCompleted;
            //}
            //mockRepository.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            //// Act
            //var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            //// Assert
            //var attributes = feedback.Select(p => new ProviderAttributeDto { Name = p.AttributeName, Value = p.AttributeValue }).ToList();

            //response.First().Should().BeEquivalentTo(new EmployerFeedbackDto
            //{
            //    Ukprn = ukprn,
            //    ProviderRating = providerRating,
            //    DateTimeCompleted = dateTimeCompleted,
            //    ProviderAttributes = attributes
            //});
            throw new NotImplementedException();
        }
    }
}