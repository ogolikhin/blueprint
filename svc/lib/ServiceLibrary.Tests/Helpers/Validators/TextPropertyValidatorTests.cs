using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Helpers.Validators
{
    [TestClass]
    public class TextPropertyValidatorTests
    {
        private TextPropertyValidator _validator;
        private const int InstancePropertyTypeId = 3;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new TextPropertyValidator();
        }

        [TestMethod]
        public void ValidateText_ReturnsNull_WhenSuccess()
        {
            // Arrange
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                TextOrChoiceValue = "Text",
            };

            var propertyType = new TextPropertyType
            {
                PropertyTypeId = 30,
                InstancePropertyTypeId = InstancePropertyTypeId,
                AllowMultiple = true,
                IsRequired = true,
                IsRichText = true,
                DefaultValue = "any"
            };

            // Act
            var result = _validator.Validate(propertyLite, new List<WorkflowPropertyType> { propertyType }, null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ValidateText_ReturnsNull_ForNonTextProperties()
        {
            // Arrange
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                TextOrChoiceValue = null,
            };

            var propertyType = new TextPropertyType
            {
                PropertyTypeId = 30,
                InstancePropertyTypeId = InstancePropertyTypeId
            };

            // Act
            var result = _validator.Validate(propertyLite, new List<WorkflowPropertyType> { propertyType }, null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ValidateText_ReturnsError_WhenNameNull()
        {
            // Arrange
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                TextOrChoiceValue = null
            };

            var propertyType = new TextPropertyType
            {
                PropertyTypeId = 30,
                InstancePropertyTypeId = InstancePropertyTypeId,
                Predefined = PropertyTypePredefined.Name
            };

            // Act
            var result = _validator.Validate(propertyLite, new List<WorkflowPropertyType> { propertyType }, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PropertySetResult));
        }

        [TestMethod]
        public void ValidateText_ReturnsError_WhenRequiredButNull()
        {
            // Arrange
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                TextOrChoiceValue = null,
            };

            var propertyType = new TextPropertyType
            {
                PropertyTypeId = 30,
                InstancePropertyTypeId = InstancePropertyTypeId,
                IsRequired = true
            };

            // Act
            var result = _validator.Validate(propertyLite, new List<WorkflowPropertyType> { propertyType }, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PropertySetResult));
        }

        [TestMethod]
        public void ValidateText_ReturnsError_WhenRequiredButEmpty()
        {
            // Arrange
            var propertyLite = new PropertyLite
            {
                PropertyTypeId = InstancePropertyTypeId,
                TextOrChoiceValue = string.Empty,
            };

            var propertyType = new TextPropertyType
            {
                PropertyTypeId = 30,
                InstancePropertyTypeId = InstancePropertyTypeId,
                IsRequired = true
            };

            // Act
            var result = _validator.Validate(propertyLite, new List<WorkflowPropertyType> { propertyType }, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PropertySetResult));
        }
    }
}
