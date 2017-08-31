using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Helpers.Validators
{
    /// <summary>
    /// Tests for the ChoicePropertyValidator
    /// </summary>
    [TestClass]
    public class ChoicePropertyValidatorTests
    {
        private ChoicePropertyValidator _validator;
        private PropertyLite _propertyLite;
        private ChoicePropertyType _propertyType;
        private List<WorkflowPropertyType> _propertyTypes;
        private const int DefaultPropertyTypeId = 10;
        private const int DefaultInstancePropertyTypeId = 20;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new ChoicePropertyValidator();
            _propertyLite = new PropertyLite
            {
                PropertyTypeId = DefaultInstancePropertyTypeId
                //,TextOrChoiceValue = "99"
            };
            _propertyLite.ChoiceIds.AddRange(new List<int> { 26 });

            _propertyType = new ChoicePropertyType
            {
                PropertyTypeId = DefaultPropertyTypeId,
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                ValidValues = new List<ValidValue>
                {
                    new ValidValue { Id = 35, Sid = 25, Value = "1" },
                    new ValidValue { Id = 36, Sid = 26, Value = "2" },
                    new ValidValue { Id = 37, Sid = 27, Value = "3" },
                    new ValidValue { Id = 38, Sid = 28, Value = "4" }
                },
                IsValidate = true,
                AllowMultiple = false
            };

            _propertyTypes = new List<WorkflowPropertyType>
            {
                _propertyType
            };
        }

        [TestMethod]
        public void Validate_ReturnsNull_When_No_Errors_Exist()
        {
            //arrange
            
            // As initialized

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_ReturnsError_When_Not_AllowMultiple_And_Has_Multiple_Coices()
        {
            //arrange
            _propertyLite.ChoiceIds.Add(27);

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsError_When_AllowMultiple_And_Has_Invalid_Choice()
        {
            //arrange
            _propertyType.AllowMultiple = true;
            _propertyLite.ChoiceIds.Add(127);

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsError_When_Not_AllowMultiple_And_Has_Invalid_Choice()
        {
            //arrange
            _propertyLite.ChoiceIds.Add(127);

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsError_When_IsRequired_And_Choices_And_TextOrChoiceValue_Are_Empty()
        {
            //arrange
            _propertyType.IsRequired = true;
            _propertyLite.ChoiceIds.Clear();

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsError_When_IsRequired_And_Not_IsValidate_And_TextOrChoiceValue_Is_Empty()
        {
            //arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidate = false;
            _propertyLite.ChoiceIds.Clear();
            _propertyLite.TextOrChoiceValue = "";

            //act
            var result = _validator.Validate(_propertyLite, _propertyTypes, null);

            //assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }
    }
}
