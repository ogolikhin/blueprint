using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    [TestClass]
    public partial class PropertyChangeActionTests
    {
        private Mock<ISaveArtifactRepository> _saveRepositoryMock;
        private Mock<IReusePropertyValidator> _reuseValidatorMock;
        private List<WorkflowPropertyType> _customPropertyTypes;

        private IExecutionParameters _executionParameters;
        private PropertyChangeAction _propertyChangeAction;

        private const int DefaultNumberInstancePropertyTypeId = 123;
        private const string DefaultNumberValue = "99";

        [TestInitialize]
        public void TestInitialize()
        {
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
        public void ValidateAction_PropertyTypeIdNotFound_ReturnsFalse()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
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
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            //Assert
            Assert.IsFalse(result);
        }



        [TestMethod]
        public void ValidateAction_UserPropertyToNumberProperty_ReturnsFalse()
        {
            //Arrange
            InitializeNumberPropertyChangeAction();
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
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
