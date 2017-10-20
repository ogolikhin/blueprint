using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public partial class PropertyChangeActionTests
    {
        private int DefaultUserInstancePropertyTypeId = 124;
        private WorkflowPropertyType DefaultUserPropertyType;
        private List<UserGroup> DefaultUserGroups = new List<UserGroup>()
        {
           new UserGroup() { Id = 1, IsGroup = true },
           new UserGroup() { Id = 1, IsGroup = false }
        };
        private void InitializeUserPropertyChangeAction()
        {
            DefaultUserPropertyType = new UserPropertyType()
            {
                InstancePropertyTypeId = DefaultUserInstancePropertyTypeId,
                PrimitiveType = PropertyPrimitiveType.User
            };
            _propertyChangeAction = new PropertyChangeUserGroupsAction()
            {
                InstancePropertyTypeId = DefaultUserInstancePropertyTypeId
            };
            ((PropertyChangeUserGroupsAction)_propertyChangeAction).UserGroups.AddRange(DefaultUserGroups);

            _customPropertyTypes = new List<WorkflowPropertyType>()
            {
                DefaultUserPropertyType
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
        public void ValidateAction_DefaultUserPropertyChangeAction_UserIsPopulated()
        {
            // Arrange
            InitializeUserPropertyChangeAction();

            // Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            // Assert
            Assert.IsNull(result);
            Assert.IsTrue(propertyLiteValue.UsersAndGroups.Any());
        }

        [TestMethod]
        public void ValidateAction_UserPropertyContainsPropertyValueFromXml_ReturnsFailureResult()
        {
            // Arrange
            InitializeUserPropertyChangeAction();
            _propertyChangeAction.PropertyValue = "some Value";

            // Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateAction_UserPropertyAsBaseClassPropertyChangeAction_ReturnsFailureResult()
        {
            // Arrange
            InitializeUserPropertyChangeAction();
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultUserInstancePropertyTypeId
            };

            // Act
            var result = _propertyChangeAction.ValidateAction(_executionParameters);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateAction_UserPropertyChangedToNumberProperty_ReturnsFailureResult()
        {
            // Act
            var result = UserPropertyChangedToAnotherPropertyValidation(
                new NumberPropertyType()
                {
                    InstancePropertyTypeId = DefaultUserInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Number
                });

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateAction_UserPropertyChangedToDateProperty_ReturnsFailureResult()
        {
            // Act
            var result = UserPropertyChangedToAnotherPropertyValidation(
                new DatePropertyType()
                {
                    InstancePropertyTypeId = DefaultUserInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Date
                });

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateAction_UserPropertyChangedToChoiceProperty_ReturnsFailureResult()
        {
            // Act
            var result = UserPropertyChangedToAnotherPropertyValidation(
                new ChoicePropertyType()
                {
                    InstancePropertyTypeId = DefaultUserInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Choice
                });

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ValidateAction_UserPropertyChangedToTextProperty_ReturnsFailureResult()
        {
            // Act
            var result = UserPropertyChangedToAnotherPropertyValidation(
                new TextPropertyType()
                {
                    InstancePropertyTypeId = DefaultUserInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Text
                });

            // Assert
            Assert.IsNotNull(result);
        }
        private PropertySetResult UserPropertyChangedToAnotherPropertyValidation(WorkflowPropertyType propertyType)
        {
            // Arrange
            InitializeUserPropertyChangeAction();
            _executionParameters.CustomPropertyTypes.Clear();
            _executionParameters.CustomPropertyTypes.Add(propertyType);

            // Act
            return _propertyChangeAction.ValidateAction(_executionParameters);
        }
    }
}
