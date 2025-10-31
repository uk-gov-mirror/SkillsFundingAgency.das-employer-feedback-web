using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Domain.Types;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Validators.Questions;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Validators.Questions
{
    [TestFixture]
    public class QuestionThreeRatingViewModelValidatorTests
    {
        private QuestionThreeRatingViewModelValidator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new QuestionThreeRatingViewModelValidator();
        }

        [Test]
        public void Should_Pass_When_Rating_Is_Selected()
        {
            // Arrange
            var model = new QuestionThreeRatingViewModel
            {
                Rating = ProviderRating.VeryPoor,
                ProviderName = "Example Provider"
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Should_Fail_When_Rating_Is_Null()
        {
            // Arrange
            var model = new QuestionThreeRatingViewModel
            {
                Rating = null,
                ProviderName = "Sample College"
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Rating)
                .WithErrorMessage("Please rate Sample College");
        }
    }
}
