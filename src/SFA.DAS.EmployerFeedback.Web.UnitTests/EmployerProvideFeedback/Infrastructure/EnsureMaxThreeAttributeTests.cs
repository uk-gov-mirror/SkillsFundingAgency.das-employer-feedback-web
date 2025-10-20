using NUnit.Framework;
using SFA.DAS.EmployerFeedback.Web.Models.Questions;
using SFA.DAS.EmployerFeedback.Web.Validators.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerFeedback.Web.UnitTests.Infrastructure
{
    [TestFixture]
    public class EnsureMaxThreeAttributeTests
    {
        private EnsureMaxThreeProviderAttribute _attribute;

        [SetUp]
        public void SetUp()
        {
            _attribute = new EnsureMaxThreeProviderAttribute();
        }

        [Test]
        public void IsValid_ReturnsFalse_ForNullValue()
        {
            // Arrange
            object value = null;

            // Act
            var result = _attribute.IsValid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IsValid_ReturnsTrue_For_List_With_UpTo_Three_GoodItems(int itemCount)
        {
            // Arrange
            List<ProviderAttributeModel> attributes = Enumerable.Range(1, itemCount)
                .Select(i => new ProviderAttributeModel { Good = true, Bad = false })
                .ToList();

            // Act
            var result = _attribute.IsValid(attributes);

            // Assert
            Assert.IsTrue(result, $"attributes with {itemCount} items should be valid (max 3 allowed).");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void IsValid_ReturnsTrue_For_List_With_UpTo_Three_BadItems(int itemCount)
        {
            // Arrange
            List<ProviderAttributeModel> attributes = Enumerable.Range(1, itemCount)
                .Select(i => new ProviderAttributeModel { Good = false, Bad = true })
                .ToList();

            // Act
            var result = _attribute.IsValid(attributes);

            // Assert
            Assert.IsTrue(result, $"attributes with {itemCount} items should be valid (max 3 allowed).");
        }

        [Test]
        public void IsValid_ReturnsFalse_For_List_With_More_Than_Three_Items()
        {
            // Arrange
            List<ProviderAttributeModel> attributes = Enumerable.Range(1, 4)
                .Select(i => new ProviderAttributeModel { Good = false, Bad = true })
                .ToList();

            // Act
            var result = _attribute.IsValid(attributes);

            // Assert
            Assert.IsFalse(result, "attributes with more than 3 items should be invalid.");
        }
    }
}

