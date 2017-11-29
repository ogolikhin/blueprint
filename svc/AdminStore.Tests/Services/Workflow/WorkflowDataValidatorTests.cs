using System.Collections.Generic;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowDataValidatorTests
    {
        private Mock<IWorkflowRepository> _workflowRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<ISqlProjectMetaRepository> _projectMetadataRepositoryMock;
        private Mock<IWorkflowActionPropertyValueValidator> _propertyValueValidatorMock;
        private Mock<WorkflowDataValidator> _dataValidatorMock;

        [TestInitialize]
        public void Initialize()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _usersRepositoryMock = new Mock<IUsersRepository>();
            _projectMetadataRepositoryMock = new Mock<ISqlProjectMetaRepository>();
            _propertyValueValidatorMock = new Mock<IWorkflowActionPropertyValueValidator>();
            _dataValidatorMock = new Mock<WorkflowDataValidator>(
                _workflowRepositoryMock.Object,
                _usersRepositoryMock.Object,
                _projectMetadataRepositoryMock.Object,
                _propertyValueValidatorMock.Object)
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
        public void ValidatePropertyChangeActionData_PropertyValueValidationSuccess_Success()
        {
            // Arrange
            const string propertyName = "some";
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName };
            var itemType = new ItemType { Id = 1, CustomPropertyTypeIds = { propertyType.Id } };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);
            result.StandardTypes.ArtifactTypes.Add(itemType);
            result.ValidArtifactTypeIds.Add(itemType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyName };
            WorkflowDataValidationErrorCodes? errorCode;

            _propertyValueValidatorMock
                .Setup(m => m.ValidatePropertyValue(action, propertyType, It.IsAny<IList<SqlUser>>(), It.IsAny<IList<SqlGroup>>(), true, out errorCode))
                .Returns(true);

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
            result.ValidArtifactTypeIds.Add(itemType.Id);
            var action = new IePropertyChangeAction { PropertyName = propertyName };
            WorkflowDataValidationErrorCodes? errorCode = WorkflowDataValidationErrorCodes.ProjectByIdNotFound;

            _propertyValueValidatorMock
                .Setup(m => m.ValidatePropertyValue(action, propertyType, It.IsAny<IList<SqlUser>>(), It.IsAny<IList<SqlGroup>>(), true, out errorCode))
                .Returns(false);

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeActionData(result, action, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
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
        public void ValidateEmailNotificationActionData_PropertyTypeNotAssociated_Error()
        {
            // Arrange
            var propertyName = "some";
            var emailNotificationAction = new IeEmailNotificationAction { PropertyName = propertyName };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName, PrimitiveType = PropertyPrimitiveType.Text };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, emailNotificationAction, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.EmailNotificationActionPropertyTypeNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateEmailNotificationActionData_PropertyTypeAssociated_Success()
        {
            // Arrange
            var propertyName = "some";
            var emailNotificationAction = new IeEmailNotificationAction { PropertyName = propertyName };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName, PrimitiveType = PropertyPrimitiveType.User };
            var itemType = new ItemType { Id = 1, CustomPropertyTypeIds = { propertyType.Id } };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);
            result.StandardTypes.ArtifactTypes.Add(itemType);
            result.ValidArtifactTypeIds.Add(itemType.Id);

            // Act
            _dataValidatorMock.Object.ValidateEmailNotificationActionData(result, emailNotificationAction, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        #endregion

        #region Events

        [TestMethod]
        public void ValidatePropertyChangeEvent_PropertyTypeNotAssociated_Error()
        {
            // Arrange
            var propertyName = "some";
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyName };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyNotAssociated, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeEvent_PropertyTypeAssociated_Success()
        {
            // Arrange
            var propertyName = "some";
            var propertyChangeEvent = new IePropertyChangeEvent { PropertyName = propertyName };
            var result = new WorkflowDataValidationResult { StandardTypes = new ProjectTypes() };
            var propertyType = new PropertyType { Id = 1, Name = propertyName };
            var itemType = new ItemType { Id = 1, CustomPropertyTypeIds = { propertyType.Id } };
            result.StandardPropertyTypeMapByName.Add(propertyName, propertyType);
            result.StandardTypes.ArtifactTypes.Add(itemType);
            result.ValidArtifactTypeIds.Add(itemType.Id);

            // Act
            _dataValidatorMock.Object.ValidatePropertyChangeEventData(result, propertyChangeEvent, true);

            // Assert
            Assert.AreEqual(false, result.HasErrors);
            Assert.AreEqual(0, result.Errors.Count);
        }

        #endregion Events
    }
}
