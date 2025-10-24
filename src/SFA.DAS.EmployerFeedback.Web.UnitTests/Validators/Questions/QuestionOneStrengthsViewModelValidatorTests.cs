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
    public class QuestionOneStrengthsViewModelValidatorTests
    {
        private QuestionOneStrengthsViewModelValidator _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new QuestionOneStrengthsViewModelValidator();
        }

        [Test]
        public void Should_Pass_When_Three_Or_Fewer_Good_Selected()
        {
            // Arrange
            var model = new QuestionOneStrengthsViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Good = true },
                    new ProviderAttributeModel { Good = true },
                    new ProviderAttributeModel { Good = true },
                    new ProviderAttributeModel { Good = false }
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void Should_Fail_With_Collection_And_PerItem_Good_Errors_When_More_Than_Three_Good_Selected()
        {
            // Arrange
            var model = new QuestionOneStrengthsViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Good = true },  // idx 0
                    new ProviderAttributeModel { Good = true },  // idx 1
                    new ProviderAttributeModel { Good = true },  // idx 2
                    new ProviderAttributeModel { Good = true },  // idx 3 -> 4th Good
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.Errors.Should().ContainSingle(e =>
                e.PropertyName == "Attributes" &&
                e.ErrorMessage == "Choose up to 3 options");

            // per-item .Good errors for every Good=true element (here: all 4)
            var perItemErrors = result.Errors.Where(e => e.PropertyName.StartsWith("Attributes[") && e.PropertyName.EndsWith("].Good")).ToList();
            perItemErrors.Should().HaveCount(4);
            perItemErrors.Should().OnlyContain(e => e.ErrorMessage == "Choose up to 3 options");

            perItemErrors.Select(e => e.PropertyName)
                .Should().BeEquivalentTo(
                    "Attributes[0].Good",
                    "Attributes[1].Good",
                    "Attributes[2].Good",
                    "Attributes[3].Good"
                );
        }

        [Test]
        public void Should_Fail_And_Flag_Only_Indices_With_Good_True()
        {
            // Arrange
            var model = new QuestionOneStrengthsViewModel
            {
                Attributes = new List<ProviderAttributeModel>
                {
                    new ProviderAttributeModel { Good = true },   // 0
                    new ProviderAttributeModel { Good = false },  // 1
                    new ProviderAttributeModel { Good = true },   // 2
                    new ProviderAttributeModel { Good = true },   // 3
                    new ProviderAttributeModel { Good = true },   // 4 -> 4th Good overall
                    new ProviderAttributeModel { Good = false }   // 5
                }
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.Errors.Should().ContainSingle(e => e.PropertyName == "Attributes" && e.ErrorMessage == "Choose up to 3 options");

            var perItemErrors = result.Errors.Where(e => e.PropertyName.EndsWith("].Good")).ToList();
            perItemErrors.Should().HaveCount(4);
            perItemErrors.Should().OnlyContain(e => e.ErrorMessage == "Choose up to 3 options");
            
            perItemErrors.Select(e => e.PropertyName)
                .Should().BeEquivalentTo(
                    "Attributes[0].Good",
                    "Attributes[2].Good",
                    "Attributes[3].Good",
                    "Attributes[4].Good"
                );
        }
    }
}
