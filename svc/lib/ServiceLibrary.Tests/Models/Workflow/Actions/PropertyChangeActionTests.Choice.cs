using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public partial class PropertyChangeActionTests
    {
        private int DefaultChoiceInstancePropertyTypeId = 224;
        private WorkflowPropertyType DefaultChoicePropertyType;
        private void InitializeChoicePropertyChangeAction()
        {
            DefaultChoicePropertyType = new ChoicePropertyType()
            {
                InstancePropertyTypeId = DefaultChoiceInstancePropertyTypeId,
                PrimitiveType = PropertyPrimitiveType.Choice
            };
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultChoiceInstancePropertyTypeId
            };
            _customPropertyTypes = new List<WorkflowPropertyType>()
            {
                DefaultChoicePropertyType
            };
        }

        [TestMethod]
        public void ValidateAction_WhenValidValuesArePopulated_ChoiceIsPopulated()
        {
            // Arrange
            InitializeChoicePropertyChangeAction();
            _propertyChangeAction.ValidValues.Add(1);
            _executionParameters = new ExecutionParameters(
                1,
                new VersionControlArtifactInfo(),
                null,
                _customPropertyTypes,
                _saveRepositoryMock.Object,
                null,
                null,
                new List<IPropertyValidator>(),
                _reuseValidatorMock.Object);

            // Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            // Assert
            Assert.IsNull(result);
            Assert.IsFalse(propertyLiteValue.ChoiceIds.IsEmpty());
        }

        [TestMethod]
        public void ValidateAction_WhenValidValuesAreEmpty_TextOrCustomChoiceIsPopulated()
        {
            // Arrange
            var defaultText = "Default Text";
            InitializeChoicePropertyChangeAction();
            _propertyChangeAction.PropertyValue = defaultText;
            _executionParameters = new ExecutionParameters(
                1,
                new VersionControlArtifactInfo(),
                null,
                _customPropertyTypes,
                _saveRepositoryMock.Object,
                null,
                null,
                new List<IPropertyValidator>(),
                _reuseValidatorMock.Object);

            // Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            // Assert
            Assert.IsNull(result);
            Assert.AreEqual(propertyLiteValue.TextOrChoiceValue, defaultText);
        }
    }
}
