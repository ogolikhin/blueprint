using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    [TestClass]
    public class DatePropertyValueValidatorTests
    {
        private WorkflowDataValidationResult _result;
        private PropertyType _propertyType;

        [TestInitialize]
        public void Initialize()
        {
            _result = new WorkflowDataValidationResult();
            _propertyType = new PropertyType { PrimitiveType = PropertyPrimitiveType.Date };
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_Required_Iso8601_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "2017-07-21" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_HasValidValues_Failure()
        {
            // Arrange
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue()
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_HasUsersGroups_Failure()
        {
            // Arrange
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup()
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_Required_ShiftInDays_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "-1" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_Required_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "  " };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_InvalidFormat_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = false;
            var action = new IePropertyChangeAction { PropertyValue = "2017-07-21T15:00:00Z" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_NotRequiredEmpty_Success()
        {
            // Arrange
            _propertyType.IsRequired = false;
            _propertyType.IsValidated = false;
            var action = new IePropertyChangeAction { PropertyValue = " " };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_InRange_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            _propertyType.MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var action = new IePropertyChangeAction { PropertyValue = "2017-08-30" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_LessThanMin_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            _propertyType.MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var action = new IePropertyChangeAction { PropertyValue = "2017-07-31" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_GreaterThanMax_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            _propertyType.MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var action = new IePropertyChangeAction { PropertyValue = "2017-08-31" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange, _result.Errors.Single().ErrorCode);
        }

        private void Validate(IePropertyChangeAction action, bool ignoreIds)
        {
            var factory = new PropertyValueValidatorFactory();
            var validator = factory.Create(_propertyType, null, null, ignoreIds);
            validator.Validate(action, _propertyType, _result);
        }
    }
}
