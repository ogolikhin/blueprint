﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Helpers.Validators
{
    /// <summary>
    /// Tests for the DatePropertyValidator
    /// </summary>
    [TestClass]
    public class DatePropertyValidatorTests
    {
        private DatePropertyValidator _validator;
        private PropertyLite _propertyLite;
        private DatePropertyType _propertyType;
        private List<WorkflowPropertyType> _propertyTypes;
        private const int DefaultPropertyTypeId = 10;
        private const int DefaultInstancePropertyTypeId = 20;
        private Mock<IValidationContext> _validationContextMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new DatePropertyValidator();
            _propertyLite = new PropertyLite
            {
                PropertyTypeId = DefaultInstancePropertyTypeId,
                DateValue = new DateTime(2018, 1, 1)
            };
            _propertyType = new DatePropertyType
            {
                PropertyTypeId = DefaultPropertyTypeId,
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                Range = new Range<DateTime?>
                {
                    Start = new DateTime(2017, 1, 1),
                    End = new DateTime(2019, 1, 1)
                },
                IsValidate = true
            };
            _propertyTypes = new List<WorkflowPropertyType>
            {
                _propertyType
            };
            _validationContextMock = new Mock<IValidationContext>();
        }

        [TestMethod]
        public void Validate_ReturnsNull_WhenNoErrorsExist()
        {
            // arrange
            var date = new DateTime(2017, 1, 1);
            _propertyLite.DateValue = date;
            _propertyType.Range.Start = date.AddYears(-1);
            _propertyType.Range.End = date.AddYears(1);

            // act
            var result = _validator.Validate(_propertyLite, _propertyTypes, _validationContextMock.Object);

            // assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Validate_ReturnsError_WhenDateIsLessThanMinimum()
        {
            // arrange
            var date = new DateTime(2017, 2, 2);
            _propertyLite.DateValue = date;
            _propertyType.Range.Start = date.AddYears(1);
            _propertyType.Range.End = date.AddYears(2);

            // act
            var result = _validator.Validate(_propertyLite, _propertyTypes, _validationContextMock.Object);

            // assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsError_WhenDateIsGreaterThanMaximum()
        {
            // arrange
            var date = new DateTime(2017, 3, 3);
            _propertyLite.DateValue = date;
            _propertyType.Range.Start = date.AddYears(-2);
            _propertyType.Range.End = date.AddYears(-1);

            // act
            var result = _validator.Validate(_propertyLite, _propertyTypes, _validationContextMock.Object);

            // assert
            Assert.AreEqual(ErrorCodes.InvalidArtifactProperty, result.ErrorCode);
        }

        [TestMethod]
        public void Validate_ReturnsNull_WhenDateIsNull()
        {
            // arrange
            _propertyLite.DateValue = null;

            // act
            var result = _validator.Validate(_propertyLite, _propertyTypes, _validationContextMock.Object);

            // assert
            Assert.IsNull(result);
        }
    }
}
