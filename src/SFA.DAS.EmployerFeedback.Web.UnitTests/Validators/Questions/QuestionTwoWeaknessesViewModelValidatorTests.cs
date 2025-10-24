using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Validators.Questions;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Validators
{
    [TestFixture]
    public class QuestionTwoWeaknessesViewModelValidatorTests
    {
        private QuestionTwoWeaknessesViewModelValidator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new QuestionTwoWeaknessesViewModelValidator();
        }

        [Test]
        public void Should_Pass_When_Three_Or_Fewer_Bad_Selected()
        {
            // Arrange
            var model = new QuestionTwoWeaknessesViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Bad = true },
                    new ProviderAttributeModel { Bad = true },
                    new ProviderAttributeModel { Bad = true },
                    new ProviderAttributeModel { Bad = false }
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Should_Fail_With_Collection_And_PerItem_Bad_Errors_When_More_Than_Three_Bad_Selected()
        {
            // Arrange
            var model = new QuestionTwoWeaknessesViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Bad = true },  // idx 0
                    new ProviderAttributeModel { Bad = true },  // idx 1
                    new ProviderAttributeModel { Bad = true },  // idx 2
                    new ProviderAttributeModel { Bad = true },  // idx 3 -> 4th Bad
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == "Attributes" &&
                e.ErrorMessage == "Choose up to 3 options");

            // per-item .Bad errors for every Bad=true element (here: all 4)
            var perItemErrors = result.Errors.Where(e => e.PropertyName.StartsWith("Attributes[") && e.PropertyName.EndsWith("].Bad")).ToList();
            perItemErrors.Should().HaveCount(4);
            perItemErrors.Should().OnlyContain(e => e.ErrorMessage == "Choose up to 3 options");

            perItemErrors.Select(e => e.PropertyName)
                .Should().BeEquivalentTo(
                    "Attributes[0].Bad",
                    "Attributes[1].Bad",
                    "Attributes[2].Bad",
                    "Attributes[3].Bad"
                );
        }

        [Test]
        public void Should_Fail_And_Flag_Only_Indices_With_Bad_True()
        {
            // Arrange
            var model = new QuestionTwoWeaknessesViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Bad = true },   // 0
                    new ProviderAttributeModel { Bad = false },  // 1
                    new ProviderAttributeModel { Bad = true },   // 2
                    new ProviderAttributeModel { Bad = true },   // 3
                    new ProviderAttributeModel { Bad = true },   // 4 -> 4th Bad overall
                    new ProviderAttributeModel { Bad = false }   // 5
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Attributes" && e.ErrorMessage == "Choose up to 3 options");

            var perItemErrors = result.Errors.Where(e => e.PropertyName.EndsWith("].Bad")).ToList();
            perItemErrors.Should().HaveCount(4);
            perItemErrors.Should().OnlyContain(e => e.ErrorMessage == "Choose up to 3 options");

            perItemErrors.Select(e => e.PropertyName)
                .Should().BeEquivalentTo(
                    "Attributes[0].Bad",
                    "Attributes[2].Bad",
                    "Attributes[3].Bad",
                    "Attributes[4].Bad"
                );
        }
    }
}
