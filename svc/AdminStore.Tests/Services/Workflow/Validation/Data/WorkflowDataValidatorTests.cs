using System.Collections.Generic;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow.Validation.Data.PropertyValue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data
{
    [TestClass]
    public class WorkflowDataValidatorTests
    {
        private Mock<IWorkflowRepository> _workflowRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<ISqlProjectMetaRepository> _projectMetadataRepositoryMock;
        private Mock<IPropertyValueValidatorFactory> _propertyValueValidatorFactoryMock;
        private Mock<IPropertyValueValidator> _propertyValueValidatorMock;
        private Mock<WorkflowDataValidator> _dataValidatorMock;

        [TestInitialize]
        public void Initialize()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _usersRepositoryMock = new Mock<IUsersRepository>();
            _projectMetadataRepositoryMock = new Mock<ISqlProjectMetaRepository>();
            _propertyValueValidatorMock = new Mock<IPropertyValueValidator>();
            _propertyValueValidatorFactoryMock = new Mock<IPropertyValueValidatorFactory>();
            _propertyValueValidatorFactoryMock
                .Setup(m => m.Create(It.IsAny<PropertyType>(), It.IsAny<IList<SqlUser>>(), It.IsAny<IList<SqlGroup>>(), It.IsAny<bool>()))
                .Returns(() => _propertyValueValidatorMock.Object);

            _dataValidatorMock = new Mock<WorkflowDataValidator>(
                _workflowRepositoryMock.Object,
                _usersRepositoryMock.Object,
                _projectMetadataRepositoryMock.Object,
                _propertyValueValidatorFactoryMock.Object)
            {
                CallBase = true
            };
        }

        #region Actions

        [TestMethod]
        public void ValidateActionData_EmailNotificationAction_CalledRespectiveValidationMethod()
        {
            // Arrange
            var result = new WorkflowDataValidationResult();
            var action = new IeEmailNotificationAction();
            _dataValidatorMock.Setup(m => m.ValidateEmailNotificationActionData(result, action, true)).Verifiable();

            // Act
            _dataValidatorMock.Object.ValidateActionData(result, action, true);

            // Assert
            _dataValidatorMock.Verify();
        }

        [TestMethod]
        public void ValidateActionData_PropertyChangeAction_CalledRespectiveValidationMethod()
        {
            // Arrange
            var result = new WorkflowDataValidationResult();
            var action = new IePropertyChangeAction();
            _dataValidatorMock.Setup(m => m.ValidatePropertyChangeActionData(result, action, true)).Verifiable();

            // Act
            _dataValidatorMock.Object.ValidateActionData(result, action, true);

            // Assert
            _dataValidatorMock.Verify();
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyTypeNotFound_Error()
        {
            // Arrange
            const string propertyName = "some";
            var result = new WorkflowDataValidationResult();
            var action = new IePropertyChangeAction
            {
                PropertyName = propertyName
            };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundByName, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyTypeNotAssociated_Error()
        {
            // Arrange
            const string propertyName = "some";
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyName, new PropertyType { Name = propertyName });
            var action = new IePropertyChangeAction { PropertyName = propertyName };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_CustomProperty_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_CustomProperty_NotAssociatedWithArtifactTypes_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_CustomProperty_AssociatedWithArtifactTypes_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            artifactType.CustomPropertyTypeIds.Add(propertyType.Id);

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_Name_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_Name_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_Description_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_Description_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyValueValidationSuccess_Success()
        {
            // Arrange
            const string propertyName = "some";
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName };
            var itemType = new ItemType { Id = 1, CustomPropertyTypeIds = { propertyType.Id } };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);
            result.StandardTypes.ArtifactTypes.Add(itemType);
            result.AssociatedArtifactTypeIds.Add(itemType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyName };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyValueValidationFailure_Error()
        {
            // Arrange
            const string propertyName = "some";
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName };
            var itemType = new ItemType { Id = 1, CustomPropertyTypeIds = { propertyType.Id } };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);
            result.StandardTypes.ArtifactTypes.Add(itemType);
            result.AssociatedArtifactTypeIds.Add(itemType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyName };
            const WorkflowDataValidationErrorCodes errorCode = WorkflowDataValidationErrorCodes.ProjectByIdNotFound;
            _propertyValueValidatorMock
                .Setup(m => m.Validate(action, propertyType, result))
                .Callback(() => result.Errors.Add(new WorkflowDataValidationError { Element = propertyName, ErrorCode = errorCode }));

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(errorCode, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateActionData_GenerateAction_CalledRespectiveValidationMethod()
        {
            // Arrange
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction();
            _dataValidatorMock.Setup(m => m.ValidateGenerateActionData(result, action, true)).Verifiable();

            // Act
            _dataValidatorMock.Object.ValidateActionData(result, action, true);

            // Assert
            _dataValidatorMock.Verify();
        }

        [TestMethod]
        public void ValidateGenerateActionData_ChildArtifacts_CalledRespectiveValidationMethod()
        {
            // Arrange
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction { GenerateActionType = GenerateActionTypes.Children };
            _dataValidatorMock.Setup(m => m.ValidateGenerateChildArtifactsActionData(result, action, true)).Verifiable();

            // Act
            _dataValidatorMock.Object.ValidateGenerateActionData(result, action, true);

            // Assert
            _dataValidatorMock.Verify();
        }

        [TestMethod]
        public void ValidateGenerateChildArtifactsActionData_ArtifcatTypeFound_Success()
        {
            // Arrange
            const string artifactType = "some";
            var result = new WorkflowDataValidationResult();
            result.StandardArtifactTypeMapByName.Add(artifactType, new ItemType());
            var action = new IeGenerateAction
            {
                GenerateActionType = GenerateActionTypes.Children,
                ArtifactType = artifactType
            };

            // Act
            _dataValidatorMock.Object.ValidateGenerateChildArtifactsActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
        }

        [TestMethod]
        public void ValidateGenerateChildArtifactsActionData_ArtifcatTypeNotFound_Error()
        {
            // Arrange
            const string artifactType = "some";
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction
            {
                GenerateActionType = GenerateActionTypes.Children,
                ArtifactType = artifactType
            };

            // Act
            _dataValidatorMock.Object.ValidateGenerateChildArtifactsActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundByName, result.Errors[0].ErrorCode);
            Assert.AreEqual(artifactType, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_CustomProperty_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property", PrimitiveType = PropertyPrimitiveType.User };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_CustomProperty_NotAssociatedWithArtifactTypes_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property", PrimitiveType = PropertyPrimitiveType.User };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_CustomProperty_AssociatedWithArtifactTypes_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property", PrimitiveType = PropertyPrimitiveType.User };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            artifactType.CustomPropertyTypeIds.Add(propertyType.Id);

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_Name_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name", PrimitiveType = PropertyPrimitiveType.Text };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_Name_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name", PrimitiveType = PropertyPrimitiveType.Text };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_Description_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description", PrimitiveType = PropertyPrimitiveType.Text };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_Description_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description", PrimitiveType = PropertyPrimitiveType.Text };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var action = new IeEmailNotificationAction { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, action, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        #endregion

        #region Events

        [TestMethod]
        public void ValidatePropertyChangeEventData_CustomProperty_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_CustomProperty_NotAssociatedWithArtifactTypes_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_CustomProperty_AssociatedWithArtifactTypes_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = 1, Name = "My Standard Property" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };


            artifactType.CustomPropertyTypeIds.Add(propertyType.Id);

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_Name_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_Name_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdName, Name = "Name" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_Description_NoArtifactTypesAssociated_Error()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyType.Name, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeEventData_Description_ArtifactTypesAssociated_NoError()
        {
            // Arrange
            var propertyType = new PropertyType { Id = WorkflowConstants.PropertyTypeFakeIdDescription, Name = "Description" };
            var artifactType = new ItemType { Id = 1 };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            result.StandardPropertyTypeMapByName.Add(propertyType.Name, propertyType);
            result.StandardTypes.ArtifactTypes.Add(artifactType);
            result.AssociatedArtifactTypeIds.Add(artifactType.Id);
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyType.Name };

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        #endregion Events
    }
}
