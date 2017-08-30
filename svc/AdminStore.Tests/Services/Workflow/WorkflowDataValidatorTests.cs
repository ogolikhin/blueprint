using System.Collections.Generic;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowDataValidatorTests
    {
        #region Actions

        [TestMethod]
        public void ValidateActionData_EmailNotificationAction_CalledRespectiveValidationMethod()
        {
            //Arrange
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IeEmailNotificationAction();
            wdvMock.Setup(m => m.ValidateEmailNotificationActionData(result, action, true)).Verifiable();

            //Act
            wdvMock.Object.ValidateActionData(result, action, true);

            //Assert
            wdvMock.Verify();
        }

        [TestMethod]
        public void ValidateActionData_PropertyChangeAction_CalledRespectiveValidationMethod()
        {
            //Arrange
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IePropertyChangeAction();
            wdvMock.Setup(m => m.ValidatePropertyChangeActionData(result, action, true)).Verifiable();

            //Act
            wdvMock.Object.ValidateActionData(result, action, true);

            //Assert
            wdvMock.Verify();
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyTypeNotFound_Error()
        {
            //Arrange
            const string propertyName = "some";
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IePropertyChangeAction
            {
                PropertyName = propertyName
            };

            //Act
            wdvMock.Object.ValidatePropertyChangeActionData(result, action, true);

            //Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionPropertyTypeNotFoundByName, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyValueValidationSuccess_Success()
        {
            //Arrange
            const string propertyName = "some";
            var pvValidatorMock = new Mock<IWorkflowActionPropertyValueValidator>();
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, pvValidatorMock.Object) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var pt = new PropertyType();
            result.StandardPropertyTypeMapByName.Add(propertyName, pt);
            var action = new IePropertyChangeAction
            {
                PropertyName = propertyName
            };
            WorkflowDataValidationErrorCodes? errorCode;

            pvValidatorMock.Setup(m => m.ValidatePropertyValue(action, pt, It.IsAny<IList<SqlUser>>(),
                It.IsAny<IList<SqlGroup>>(), true, out errorCode)).Returns(true);

            //Act
            wdvMock.Object.ValidatePropertyChangeActionData(result, action, true);

            //Assert
            Assert.AreEqual(false, result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyChangeActionData_PropertyValueValidationFailure_Error()
        {
            //Arrange
            const string propertyName = "some";
            var pvValidatorMock = new Mock<IWorkflowActionPropertyValueValidator>();
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, pvValidatorMock.Object) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var pt = new PropertyType();
            result.StandardPropertyTypeMapByName.Add(propertyName, pt);
            var action = new IePropertyChangeAction
            {
                PropertyName = propertyName
            };
            WorkflowDataValidationErrorCodes? errorCode = WorkflowDataValidationErrorCodes.ProjectByIdNotFound;

            pvValidatorMock.Setup(m => m.ValidatePropertyValue(action, pt, It.IsAny<IList<SqlUser>>(),
                It.IsAny<IList<SqlGroup>>(), true, out errorCode)).Returns(false);

            //Act
            wdvMock.Object.ValidatePropertyChangeActionData(result, action, true);

            //Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(errorCode, result.Errors[0].ErrorCode);
            Assert.AreEqual(propertyName, result.Errors[0].Element as string);
        }

        [TestMethod]
        public void ValidateActionData_GenerateAction_CalledRespectiveValidationMethod()
        {
            //Arrange
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction();
            wdvMock.Setup(m => m.ValidateGenerateActionData(result, action, true)).Verifiable();

            //Act
            wdvMock.Object.ValidateActionData(result, action, true);

            //Assert
            wdvMock.Verify();
        }

        [TestMethod]
        public void ValidateGenerateActionData_ChildArtifacts_CalledRespectiveValidationMethod()
        {
            //Arrange
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction
            {
                GenerateActionType = GenerateActionTypes.Children
            };
            wdvMock.Setup(m => m.ValidateGenerateChildArtifactsActionData(result, action, true)).Verifiable();

            //Act
            wdvMock.Object.ValidateGenerateActionData(result, action, true);

            //Assert
            wdvMock.Verify();
        }

        [TestMethod]
        public void ValidateGenerateChildArtifactsActionData_ArtifcatTypeFound_Success()
        {
            //Arrange
            const string artifactType = "some";
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            result.StandardArtifactTypeMapByName.Add(artifactType, new ItemType());
            var action = new IeGenerateAction
            {
                GenerateActionType = GenerateActionTypes.Children,
                ArtifactType = artifactType
            };

            //Act
            wdvMock.Object.ValidateGenerateChildArtifactsActionData(result, action, true);

            //Assert
            Assert.AreEqual(false, result.HasErrors);
        }

        [TestMethod]
        public void ValidateGenerateChildArtifactsActionData_ArtifcatTypeNotFound_Error()
        {
            //Arrange
            const string artifactType = "some";
            var wdvMock = new Mock<WorkflowDataValidator>(null, null, null, null) { CallBase = true };
            var result = new WorkflowDataValidationResult();
            var action = new IeGenerateAction
            {
                GenerateActionType = GenerateActionTypes.Children,
                ArtifactType = artifactType
            };

            //Act
            wdvMock.Object.ValidateGenerateChildArtifactsActionData(result, action, true);

            //Assert
            Assert.AreEqual(true, result.HasErrors);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.GenerateChildArtifactsActionArtifactTypeNotFoundByName, result.Errors[0].ErrorCode);
            Assert.AreEqual(artifactType, result.Errors[0].Element as string);
        }

        #endregion
    }
}
