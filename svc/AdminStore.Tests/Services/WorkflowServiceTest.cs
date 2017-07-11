using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;
using ServiceLibrary.Models;
using System.Collections;
using System.Data;

namespace AdminStore.Services
{
    [TestClass]
    public class WorkflowServiceTest
    {
        #region Vars

        private Mock<IWorkflowValidator> _workflowValidatorMock;
        private Mock<IWorkflowRepository> _workflowRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;

        private WorkflowService _service;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;
        private const InstanceAdminPrivileges AllProjectDataPermissions = InstanceAdminPrivileges.AccessAllProjectData;

        #endregion
        [TestInitialize]
        public void Initialize()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _workflowValidatorMock = new Mock<IWorkflowValidator>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new WorkflowService(_workflowRepositoryMock.Object, _workflowValidatorMock.Object, _userRepositoryMock.Object);
        }

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowValidator>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var workflowService = new WorkflowService(workflowRepositoryMock.Object, workflowValidatorMock.Object,
                userRepositoryMock.Object);
            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowArtifactTypesAndProjects = new List<SqlWorkflowArtifactTypesAndProjects>
            {
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 1,
                    ProjectName = "Project1",
                    ArtifactName = "Artifact1"
                },
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 1,
                    ProjectName = "Project1",
                    ArtifactName = "Artifact2"
                },
                new SqlWorkflowArtifactTypesAndProjects
                {
                    ProjectId = 2,
                    ProjectName = "Project2",
                    ArtifactName = "Artifact2"
                }
            };

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAndProjectsAsync(It.IsAny<int>())).ReturnsAsync(workflowArtifactTypesAndProjects);

            //act
            var workflowDetails = await workflowService.GetWorkflowDetailsAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowDetails);
            Assert.AreEqual(2, workflowDetails.Projects.Count());
            Assert.AreEqual(2, workflowDetails.ArtifactTypes.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetWorkflow_ThereIsNoSuchWorkflow_NotFoundResult()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowValidator>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var workflowService = new WorkflowService(workflowRepositoryMock.Object, workflowValidatorMock.Object,
                userRepositoryMock.Object);
            var workflowId = 10;

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);

            //act
            await workflowService.GetWorkflowDetailsAsync(workflowId);
        }

        #endregion

        #region UpdateWorkflowStatusAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateWorkflowStatusAsync_WorkflowDtoIsNull_BadRequestResult()
        {
            //arrange
            WorkflowDto workflowDto = null;
                await _service.UpdateWorkflowStatusAsync(workflowDto, WorkflowId, SessionUserId);
        }
        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateWorkflowStatusAsync_RepositoryReturnsNull_NotFoundResult()
        {
            // Arrange
            WorkflowDto existingWorkflow = new WorkflowDto { VersionId = 45, WorkflowId = WorkflowId, Status = true };
            var resourceNotFoundExeption = new ResourceNotFoundException(ErrorMessages.WorkflowNotExist);
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).Throws(resourceNotFoundExeption);
            // Act
            try
            {
                await _service.UpdateWorkflowStatusAsync(existingWorkflow, WorkflowId, SessionUserId);
            }
            catch (Exception exception)
            {
                // Assert
                Assert.IsInstanceOfType(exception, typeof(ResourceNotFoundException));
                throw;
            }
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateWorkflowStatusAsync_WorkflowHasDifferentVersion_ReturnConflicErrorResult()
        {
            // Arrange
            var _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            var existingWorkflow = new WorkflowDto { VersionId = 1, WorkflowId = 1 };
            var workflowDto = new WorkflowDto { VersionId = 2, WorkflowId = 1 };

            var conflictExeption = new ConflictException(ErrorMessages.WorkflowVersionsNotEqual);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).Throws(conflictExeption);
            // Act
                await _service.UpdateWorkflowStatusAsync(workflowDto, WorkflowId, SessionUserId);
          // Assert
        }
        [TestMethod]
        public async Task UpdateWorkflowStatusAsync_RevisionIsNegative_ReturnConflicErrorResult()
        {
            // Arrange
            var _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _privilegesRepositoryMock
                    .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                    .ReturnsAsync(AllProjectDataPermissions);
            var workflowDto = new WorkflowDto { VersionId = 1, WorkflowId = 1 };
            var workflowSql = new SqlWorkflow
            {
                VersionId = 1,
                WorkflowId = 1,
                Active = true
            };

            var revisionIsNegativen = new BadRequestException(ErrorMessages.WorkflowWasNotUpdated);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflowSql);
            _workflowRepositoryMock.Setup(repo => repo.CreateRevisionInTransactionAsync(It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<string>())).Throws(revisionIsNegativen);
            //// Act
                await _service.UpdateWorkflowStatusAsync(workflowDto, WorkflowId, SessionUserId);
          // Assert
        }
        [TestMethod]
        public async Task UpdateWorkflowStatusAsync_WorkflowWasNotUpdated_ReturnBadRequestResult()
        {
            // Arrange
            var _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            int publishRevision = 12;
            IEnumerable<SqlWorkflow> updatedWorkflows = new List<SqlWorkflow>();
            var workflowDto = new WorkflowDto { VersionId = 1, WorkflowId = 100, Status = true, Name = "workflowDto1", Description = "description1" };
            var workflowSql = new SqlWorkflow
            {
                VersionId = 1,
                WorkflowId = 100,
                Active = true
            };
            var workflowWasNotUpdated = new BadRequestException(ErrorMessages.WorkflowWasNotUpdated);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflowSql);
            _workflowRepositoryMock.Setup(repo => repo.CreateRevisionInTransactionAsync(It.IsAny<IDbTransaction>(), It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(publishRevision);
            _workflowRepositoryMock.Setup(repo => repo.UpdateWorkflows(It.IsIn<IEnumerable<SqlWorkflow>>(), It.IsAny<int>(), It.IsAny<IDbTransaction>())).Throws(workflowWasNotUpdated);

            // Act
              await _service.UpdateWorkflowStatusAsync(workflowDto, WorkflowId, SessionUserId);
           // Assert
        }
        #endregion
    }
}
