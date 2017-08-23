using System.Collections.Generic;
using ArtifactStore.Models.PropertyTypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;

namespace ArtifactStore.Helpers.Validators
{
    [TestClass]
    public class NumberPropertyValidatorTests
    {
        #region Fields and constants

        private PropertyLite _property;
        private DNumberPropertyType _propertyType;
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
            _propertyType = new DNumberPropertyType
            {
                PropertyTypeId = DefaultPropertyTypeId,
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                Range = new Range<decimal> { Start = 10, End = 20 },
                DecimalPlaces = 2,
                IsValidate = true
            };
        }

        [TestMethod]
        public void ValidateNumberPropertySuccess()
        {
            //Arrange.
            var validator = new NumberPropertyValidator();

            //Act.
            var actualResult = validator.Validate(
                _property, 
                new List<DPropertyType>()
                {
                    _propertyType
                });

            //Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }
    }
}
