using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    [TestClass]
    public class NumberPropertyValueValidatorTests
    {
        private WorkflowDataValidationResult _result;
        private PropertyType _propertyType;

        [TestInitialize]
        public void Initialize()
        {
            _result = new WorkflowDataValidationResult();
            _propertyType = new PropertyType { PrimitiveType = PropertyPrimitiveType.Number };
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_Required_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "11.22" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_HasValidValues_Failure()
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
        public void ValidatePropertyValue_Number_HasUsersGroups_Failure()
        {
            // Arrange
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups { UsersGroups = new List<IeUserGroup> { new IeUserGroup() } }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNotUserPropertyUsersGroupsNotApplicable, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_Required_Failure()
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
        public void ValidatePropertyValue_Number_InvalidFormat_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = false;
            var action = new IePropertyChangeAction { PropertyValue = "aaa" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_NotRequiredEmpty_Success()
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
        public void ValidatePropertyValue_Number_InvalidDecimalPlaces_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.DecimalPlaces = 2;
            _propertyType.MinNumber = -10.10m;
            _propertyType.MaxNumber = 20.20m;
            var action = new IePropertyChangeAction { PropertyValue = "11.222" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_InRange_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.DecimalPlaces = 2;
            _propertyType.MinNumber = -10.10m;
            _propertyType.MaxNumber = 20.20m;
            var action = new IePropertyChangeAction { PropertyValue = "20.20" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_LessThanMin_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.DecimalPlaces = 2;
            _propertyType.MinNumber = -10.10m;
            _propertyType.MaxNumber = 20.20m;
            var action = new IePropertyChangeAction { PropertyValue = "-10.11" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_GreaterThanMax_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.DecimalPlaces = 2;
            _propertyType.MinNumber = -10.10m;
            _propertyType.MaxNumber = 20.20m;
            var action = new IePropertyChangeAction { PropertyValue = "20.21" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange, _result.Errors.Single().ErrorCode);
        }

        private void Validate(IePropertyChangeAction action, bool ignoreIds)
        {
            var factory = new PropertyValueValidatorFactory();
            var validator = factory.Create(_propertyType, null, null, ignoreIds);
            validator.Validate(action, _propertyType, _result);
        }
    }
}
