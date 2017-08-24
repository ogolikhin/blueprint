using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models.PropertyTypes;
using ArtifactStore.Repositories;
using BluePrintSys.Messaging.CrossCutting.Models.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Models.Workflow.Actions
{
    [TestClass]
    public class PropertyChangeActionTests
    {
        private Mock<ISaveArtifactRepository> _saveRepositoryMock;
        private Mock<IReusePropertyValidator> _reuseValidatorMock;
        private List<DPropertyType> _customPropertyTypes;

        private IExecutionParameters _executionParameters;
        private PropertyChangeAction _propertyChangeAction;

        private const int DefaultInstancePropertyTypeId = 123;
        private const string DefaultValue = "99";

        [TestInitialize]
        public void TestInitialize()
        {
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                PropertyValue = DefaultValue
            };
            _saveRepositoryMock = new Mock<ISaveArtifactRepository>();
            _reuseValidatorMock = new Mock<IReusePropertyValidator>();
            _customPropertyTypes = new List<DPropertyType>();
            _customPropertyTypes.Add(
                new DNumberPropertyType()
                {
                    InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Number
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task Execute_PropertyTypeIdNotFound_ThrowsException()
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
                new List<IPropertyValidator>(),
                _reuseValidatorMock.Object);

            //Act
            await _propertyChangeAction.Execute(_executionParameters);
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
                new List<IPropertyValidator>(), 
                _reuseValidatorMock.Object);

            //Act
            var result = await _propertyChangeAction.Execute(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }
    }
}
