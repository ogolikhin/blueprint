using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data
{
    [TestClass]
    public class TextPropertyValueValidatorTests
    {
        private WorkflowDataValidationResult _result;
        private PropertyValueValidator _propertyValueValidator;
        private PropertyType _propertyType;

        [TestInitialize]
        public void Initialize()
        {
            _result = new WorkflowDataValidationResult();
            _propertyValueValidator = new PropertyValueValidator();
            _propertyType = new PropertyType { PrimitiveType = PropertyPrimitiveType.Text };
        }

        [TestMethod]
        public void ValidatePropertyValue_Text_Required_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "value" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_Text_HasValidValues_Failure()
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
        public void ValidatePropertyValue_Text_HasUsersGroups_Failure()
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
        public void ValidatePropertyValue_Text_Required_Failure()
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

        private void Validate(IePropertyChangeAction action, bool ignoreIds)
        {
            _propertyValueValidator.Validate(action, _propertyType, null, null, ignoreIds, _result);
        }
    }
}
