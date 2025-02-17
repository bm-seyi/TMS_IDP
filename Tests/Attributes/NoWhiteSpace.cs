using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;
using TMS_IDP.Attributes;
using System.Collections.Generic;

namespace TMS_IDP.Tests.Attributes
{
    [TestClass]
    public class NoWhiteSpaceOrEmptyAttributeTests
    {
        private NoWhiteSpaceOrEmptyAttribute _attribute = null!;

        [TestInitialize]
        public void Setup()
        {
            _attribute = new NoWhiteSpaceOrEmptyAttribute();
        }

        [TestMethod]
        public void IsValid_ShouldReturnSuccess_WhenValueIsValidString()
        {
            // Arrange
            var validValue = "ValidString";
            var validationContext = new ValidationContext(new { Name = validValue });

            // Act
            var result = _attribute.GetValidationResult(validValue, validationContext);

            // Assert
            Assert.IsNull(result); // Success case returns null
        }

        [TestMethod]
        public void IsValid_ShouldReturnError_WhenValueIsEmptyString()
        {
            // Arrange
            var emptyValue = "";
            var validationContext = new ValidationContext(new { Name = emptyValue });

            // Act
            var result = _attribute.GetValidationResult(emptyValue, validationContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual($"{validationContext.DisplayName} cannot be empty or whitespace", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnError_WhenValueIsWhitespace()
        {
            // Arrange
            var whitespaceValue = "   ";
            var validationContext = new ValidationContext(new { Name = whitespaceValue });

            // Act
            var result = _attribute.GetValidationResult(whitespaceValue, validationContext);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual($"{validationContext.DisplayName} cannot be empty or whitespace", result.ErrorMessage);
        }

        [TestMethod]
        public void IsValid_ShouldReturnSuccess_WhenValueIsNull()
        {
            // Arrange
            string nullValue = null!;
            var validationContext = new ValidationContext(new { Name = nullValue });

            // Act
            var result = _attribute.GetValidationResult(nullValue, validationContext);

            // Assert
            Assert.IsNull(result); // Null values are allowed
        }

        [TestMethod]
        public void IsValid_ShouldReturnSuccess_WhenValueIsNonStringType()
        {
            // Arrange
            int intValue = 123;
            var validationContext = new ValidationContext(new { Name = intValue });

            // Act
            var result = _attribute.GetValidationResult(intValue, validationContext);

            // Assert
            Assert.IsNull(result); // Non-string values should not trigger validation
        }
    }
}
