using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    [TestClass]
    public class ChoicePropertyValueValidatorTests
    {
        private WorkflowDataValidationResult _result;
        private PropertyType _propertyType;

        [TestInitialize]
        public void Initialize()
        {
            _result = new WorkflowDataValidationResult();
            _propertyType = new PropertyType { PrimitiveType = PropertyPrimitiveType.Choice };
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_Required_Success()
        {
            // Arrange
            const string valueValue = "valid value";
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 11, Value = valueValue }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = valueValue }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_HasUsersGroups_Failure()
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
        public void ValidatePropertyValue_Choice_Required_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = false;
            var action = new IePropertyChangeAction { PropertyValue = "  " };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_MultipleNotAllowed_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsMultipleAllowed = false;
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue(),
                    new IeValidValue()
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionChoicePropertyMultipleValidValuesNotAllowed, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_UniqueValidValues_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsMultipleAllowed = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 1, Value = "value1" },
                new ValidValue { Id = 2, Value = "value2" }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = "value1" },
                    new IeValidValue { Value = "value2" }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_DuplicateValidValues_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsMultipleAllowed = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 1, Value = "value1" },
                new ValidValue { Id = 2, Value = "value2" }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = "value2" },
                    new IeValidValue { Value = "value2" }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateValidValueFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_WithIds_DuplicateValidValues_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsMultipleAllowed = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 1, Value = "value" },
                new ValidValue { Id = 2, Value = "value" }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Id = 1, Value = "value" },
                    new IeValidValue { Id = 1, Value = "value" }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateValidValueFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_WithIds_UniqueValidValues_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            _propertyType.IsMultipleAllowed = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 1, Value = "value" },
                new ValidValue { Id = 2, Value = "value" }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Id = 1, Value = "value" },
                    new IeValidValue { Id = 2, Value = "value" }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidatedSpecifiedAsNotValidated_Failure()
        {
            // Arrange
            const string valueValue = "valid value";
            _propertyType.IsValidated = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Value = valueValue }
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "value"
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotFoundByValue_Failure()
        {
            // Arrange
            const string valueValue = "valid value";
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 22, Value = valueValue }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = "invalid valid value" }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundByValue, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotFoundById_Failure()
        {
            // Arrange
            const string valueValue = "valid value";
            _propertyType.IsRequired = true;
            _propertyType.IsValidated = true;
            _propertyType.ValidValues = new List<ValidValue>
            {
                new ValidValue { Id = 22, Value = valueValue }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Id = 33, Value = "invalid valid value" }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById, _result.Errors.Single().ErrorCode);
        }

        private void Validate(IePropertyChangeAction action, bool ignoreIds)
        {
            var factory = new PropertyValueValidatorFactory();
            var validator = factory.Create(_propertyType, null, null, ignoreIds);
            validator.Validate(action, _propertyType, _result);
        }
    }
}
