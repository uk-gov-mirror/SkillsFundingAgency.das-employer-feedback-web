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
    using SFA.DAS.EmployerFeedback.Application.Queries;
    using SFA.DAS.EmployerFeedback.Domain.Entities.Models;
    using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
    using SFA.DAS.EmployerFeedback.Infrastructure.Api.Responses;

    public class FeedbackQueryTests
    {
        private readonly FeedbackQueryHandler handler;
        private readonly Mock<IEmployerFeedbackOuterApi> employerFeedbackOuterApi;
        private readonly Mock<ILogger<FeedbackQueryHandler>> mockLogger;

        public FeedbackQueryTests()
        {
            mockLogger = new Mock<ILogger<FeedbackQueryHandler>>();
            employerFeedbackOuterApi = new Mock<IEmployerFeedbackOuterApi>();
            handler = new FeedbackQueryHandler(employerFeedbackOuterApi.Object, mockLogger.Object);
        }


        [Test]
        public async Task WhenQueryingFeedback_IfNullReturnsEmptyCollection()
        {
            // Arrange
            employerFeedbackOuterApi.Setup(s => s.GetEmployerFeedback()).ReturnsAsync((IEnumerable<EmployerFeedbackViewModel>)null);

            // Act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // Assert
            response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
        }

        [Test]
        public async Task WhenQueryingFeedback_IfNoFeedbackReturnsEmptyCollection()
        {
            // Arrange
            employerFeedbackOuterApi.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(new List<EmployerFeedbackViewModel>());

            // Act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // Assert
            response.Should().BeEquivalentTo(Enumerable.Empty<EmployerFeedbackDto>());
        }

        [Test]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsConvertedModel()
        {
            // Arrange
            var fixture = new Fixture();
            var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(150);
            employerFeedbackOuterApi.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            // Act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // Assert
            response.Should().BeEquivalentTo(feedback.Select(s => new EmployerFeedbackDto
            {
                Ukprn = s.Ukprn,
                ProviderRating = s.ProviderRating,
                DateTimeCompleted = s.DateTimeCompleted,
                ProviderAttributes = new List<ProviderAttributeDto> { new ProviderAttributeDto { Name = s.AttributeName, Value = s.AttributeValue } }
            }));
        }

        [Test, AutoData]
        public async Task WhenQueryingFeedback_IfFeedbackExistsReturnsGroupedFeedback(Guid Id, long ukprn, string providerRating, DateTime dateTimeCompleted)
        {
            // Arrange
            var fixture = new Fixture();
            var feedback = fixture.CreateMany<EmployerFeedbackViewModel>(10);
            foreach (var f in feedback)
            {
                f.Id = Id;
                f.Ukprn = ukprn;
                f.ProviderRating = providerRating;
                f.DateTimeCompleted = dateTimeCompleted;
            }
            employerFeedbackOuterApi.Setup(s => s.GetEmployerFeedback()).ReturnsAsync(feedback);

            // Act
            var response = await handler.Handle(new FeedbackQuery(), new CancellationToken());

            // Assert
            var attributes = feedback.Select(p => new ProviderAttributeDto { Name = p.AttributeName, Value = p.AttributeValue }).ToList();

            response.First().Should().BeEquivalentTo(new EmployerFeedbackDto
            {
                Ukprn = ukprn,
                ProviderRating = providerRating,
                DateTimeCompleted = dateTimeCompleted,
                ProviderAttributes = attributes
            });
        }
    }
}