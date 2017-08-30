using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    [TestClass]
    public class PropertyChangeActionTests
    {
        private Mock<ISaveArtifactRepository> _saveRepositoryMock;
        private Mock<IReusePropertyValidator> _reuseValidatorMock;
        private List<WorkflowPropertyType> _customPropertyTypes;

        private IExecutionParameters _executionParameters;
        private PropertyChangeAction _propertyChangeAction;

        private const int DefaultNumberInstancePropertyTypeId = 123;
        private const string DefaultValue = "99";

        [TestInitialize]
        public void TestInitialize()
        {
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultNumberInstancePropertyTypeId,
                PropertyValue = DefaultValue
            };
            _saveRepositoryMock = new Mock<ISaveArtifactRepository>();
            _reuseValidatorMock = new Mock<IReusePropertyValidator>();
            _customPropertyTypes = new List<WorkflowPropertyType>();
            _customPropertyTypes.Add(
                new NumberPropertyType()
                {
                    InstancePropertyTypeId = DefaultNumberInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Number
                });
        }

        [TestMethod]
        public async Task Execute_PropertyTypeIdNotFound_ReturnsFalse()
        {
            //Arrange
            _propertyChangeAction.InstancePropertyTypeId = 5;
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

            //Act
            var result = await _propertyChangeAction.Execute(_executionParameters);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task Execute_NumberPropertyChangeAction_NumberIsPopulated()
        {
            //Arrange
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

            //Act
            var result = await _propertyChangeAction.Execute(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }
        [TestMethod]
        public async Task Execute_WhenNumberIsNull_NumberIsPopulatedWithNull()
        {
            //Arrange
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
            _propertyChangeAction.PropertyValue = null;

            //Act
            var result = await _propertyChangeAction.Execute(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(!propertyLiteValue.NumberValue.HasValue);
        }

        [TestMethod]
        public async Task Execute_WhenNumberIsNegative_NumberIsPopulated()
        {
            //Arrange
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
            var result = await _propertyChangeAction.Execute(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }
    }
}
