﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Helpers.Validators
{
    [TestClass]
    public class NumberPropertyValidatorTests
    {
        #region Fields and constants

        private PropertyLite _property;
        private NumberPropertyType _propertyType;
        private int DefaultPropertyTypeId = 2;
        private int DefaultInstancePropertyTypeId = 1;
        #endregion
        [TestInitialize]
        public void TestInitialize()
        {
            _property = new PropertyLite
            {
                PropertyTypeId = DefaultInstancePropertyTypeId,
                NumberValue = 15.01M
            };
            _propertyType = new NumberPropertyType
            {
                PropertyTypeId = DefaultPropertyTypeId,
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                Range = new Range<decimal> { Start = 10, End = 20 },
                DecimalPlaces = 2,
                IsValidate = true
            };
        }

        [TestMethod]
        public void Validate_DefaultNumberProperty_Success()
        {
            // Arrange.
            var validator = new NumberPropertyValidator();

            // Act.
            var actualResult = validator.Validate(
                _property,
                new List<WorkflowPropertyType>()
                {
                    _propertyType
                },
                new ValidationContext(new List<SqlUser>(), new List<SqlGroup>()));

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_DoNotValidateNumberGreaterThanRange_Success()
        {
            // Arrange.
            var validator = new NumberPropertyValidator();
            _propertyType.IsValidate = false;
            _property.NumberValue = _propertyType.Range.End + 1;
            // Act.
            var actualResult = validator.Validate(
                _property,
                new List<WorkflowPropertyType>()
                {
                    _propertyType
                },
                new ValidationContext(new List<SqlUser>(), new List<SqlGroup>()));

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_DoNotValidateNumberLessThanRange_Success()
        {
            // Arrange.
            var validator = new NumberPropertyValidator();
            _propertyType.IsValidate = false;
            _property.NumberValue = _propertyType.Range.Start - 1;
            // Act.
            var actualResult = validator.Validate(
                _property,
                new List<WorkflowPropertyType>()
                {
                    _propertyType
                },
                new ValidationContext(new List<SqlUser>(), new List<SqlGroup>()));

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_DoNotValidateMoreThanDecimalPlaces_Success()
        {
            // Arrange.
            var validator = new NumberPropertyValidator();
            _propertyType.IsValidate = false;
            _propertyType.DecimalPlaces = 2;
            _property.NumberValue = (decimal)1.1234567890;
            // Act.
            var actualResult = validator.Validate(
                _property,
                new List<WorkflowPropertyType>()
                {
                    _propertyType
                },
                new ValidationContext(new List<SqlUser>(), new List<SqlGroup>()));

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }
        [TestMethod]
        public void Validate_ValueExceedsDecimalPlaces_ReturnsErrorResultSet()
        {
            // Arrange.
            _property.NumberValue = (decimal)10.123;

            ExecuteErrorResultTests();
        }
        [TestMethod]
        public void Validate_ValuGreaterThanMaximum_ReturnsErrorResultSet()
        {
            // Arrange.
            _property.NumberValue = 21;

            ExecuteErrorResultTests();
        }
        [TestMethod]
        public void Validate_ValueLessThanMinimum_ReturnsErrorResultSet()
        {
            // Arrange.
            _property.NumberValue = 9;

            ExecuteErrorResultTests();
        }

        [TestMethod]
        public void Validate_ValueEmptyForRequiredType_ReturnsErrorResultSet()
        {
            // Arrange.
            _propertyType.IsRequired = true;
            _property.NumberValue = null;

            ExecuteErrorResultTests();
        }

        private void ExecuteErrorResultTests()
        {
            var validator = new NumberPropertyValidator();
            // Act
            var actualResult = validator.Validate(
                _property,
                new List<WorkflowPropertyType>()
                {
                    _propertyType
                },
                new ValidationContext(new List<SqlUser>(), new List<SqlGroup>()));

            // Assert
            Assert.AreEqual(actualResult.ErrorCode, ErrorCodes.InvalidArtifactProperty,
                "Error code is not InvalidArtifactProperty");
        }
    }
}
