using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public partial class PropertyChangeActionTests
    {
        private void InitializeNumberPropertyChangeAction()
        {
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultNumberInstancePropertyTypeId,
                PropertyValue = DefaultNumberValue
            };
            _customPropertyTypes = new List<WorkflowPropertyType>()
            {
                new NumberPropertyType()
                {
                    InstancePropertyTypeId = DefaultNumberInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Number
                }
            };
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
        }
        [TestMethod]
        public void ValidateAction_NumberPropertyChangeAction_NumberIsPopulated()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();

            //Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }

        [TestMethod]
        public void ValidateAction_WhenNumberIsNull_NumberIsPopulatedWithNull()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
            _propertyChangeAction.PropertyValue = null;

            //Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(!propertyLiteValue.NumberValue.HasValue);
        }

        [TestMethod]
        public void ValidateAction_WhenNumberIsNegative_NumberIsPopulated()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
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
            _propertyChangeAction.PropertyValue = "-10";

            //Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }

        [TestMethod]
        public void ValidateAction_WhenValidValuesPopulatedForNumber_ReturnsFalse()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
            _propertyChangeAction.PropertyValue = "10";
            _propertyChangeAction.ValidValues.Add(1);

            //Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ValidateAction_WhenValueIsNotNumberFormat_ReturnsFalse()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
            _propertyChangeAction.PropertyValue = "abc";

            //Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
