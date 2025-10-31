using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Models.Provider;
using SFA.DAS.EmployerFeedback.Web.Validators;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Validators
{
    [TestFixture]
    public class ProviderConfirmViewModelValidatorTests
    {
        private ProviderConfirmViewModelValidator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new ProviderConfirmViewModelValidator();
        }

        [Test]
        public void Should_Fail_When_Confirmed_Is_Null()
        {
            // Arrange
            var model = new ProviderConfirmViewModel { Confirmed = null };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Confirmed)
                .WithErrorMessage("Please choose an option");
        }

        [Test]
        public void Should_Pass_When_Confirmed_Is_True()
        {
            // Arrange
            var model = new ProviderConfirmViewModel { Confirmed = true };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Should_Pass_When_Confirmed_Is_False()
        {
            // Arrange
            var model = new ProviderConfirmViewModel { Confirmed = false };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
