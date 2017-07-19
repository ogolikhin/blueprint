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

namespace AdminStore.Services
{
    [TestClass]
    public class WorkflowServiceTest
    {
        #region Vars

        private Mock<IWorkflowXmlValidator> _workflowValidatorMock;
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
            _workflowValidatorMock = new Mock<IWorkflowXmlValidator>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _service = new WorkflowService(_workflowRepositoryMock.Object, _workflowValidatorMock.Object, _userRepositoryMock.Object);
        }

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowXmlValidator>();
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
            var workflowValidatorMock = new Mock<IWorkflowXmlValidator>();
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
        public async Task UpdateWorkflowStatusAsync_WorkflowNotExistsInDb_NotFoundResult()
        {
            // Arrange
            var existingWorkflow = new WorkflowDto { VersionId = 45, Status = true };
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);
            // Act
                await _service.UpdateWorkflowStatusAsync(existingWorkflow, WorkflowId, SessionUserId);
            // Assert
            // Exception
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateWorkflowStatusAsync_WorkflowHasDifferentVersion_ReturnConflicErrorResult()
        {
            // Arrange
            var existingWorkflow = new SqlWorkflow { VersionId = 1, WorkflowId = 1 };
            var workflowDto = new WorkflowDto { VersionId = 2, WorkflowId = 1 };
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(existingWorkflow);
            // Act
                await _service.UpdateWorkflowStatusAsync(workflowDto, WorkflowId, SessionUserId);
            // Assert
            // Exception
        }
        #endregion

        #region GetWorkflowExportAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetWorkflowExportAsync_WorkflowNotExistsInDb_NotFoundResult()
        {
            // Arrange
            var workflowId = 10;
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);
            // Act
            await _service.GetWorkflowExportAsync(workflowId);
            // Assert
            // Exception
        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_WorkflowExists_ReturnWorkflow()
        {
            //arrange
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
                    ProjectId = 2,
                    ProjectName = "Project2",
                    ArtifactName = "Artifact2"
                }
            };

            var workflowStates = new SqlState {Name = "new", Description = "newState", Default = true };
            var workflowsList = new List<SqlState> { workflowStates };

            var workflowTransitionsAndPropertyChanges = new List<SqlWorkflowTransitionsAndPropertyChanges>
            {
                new SqlWorkflowTransitionsAndPropertyChanges
                {
                    WorkflowId = 10,
                    Name = "FirsTrigger",
                    Description = "description about trigger",
                    FromState = "new",
                    ToState = "Active",
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1
                },
                new SqlWorkflowTransitionsAndPropertyChanges
                {
                    WorkflowId = 10,
                    Name = "second Trigger",
                    Description = "description about trigger",
                    FromState = "Active",
                    Permissions = "<P S=\"0\"/>",
                    Type = 1
                }
            };

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesByWorkflowId(It.IsAny<int>())).ReturnsAsync(workflowsList);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAndProjectsAsync(It.IsAny<int>())).ReturnsAsync(workflowArtifactTypesAndProjects);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowTransitionsAndPropertyChangesByWorkflowId(It.IsAny<int>())).ReturnsAsync(workflowTransitionsAndPropertyChanges);

            //act
            var workflowExport = await _service.GetWorkflowExportAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowExport);
            Assert.AreEqual(2, workflowExport.Projects.Count());
            Assert.AreEqual(2, workflowExport.ArtifactTypes.Count());
            Assert.AreEqual(1, workflowExport.States.Count());
            Assert.AreEqual(2, workflowExport.Triggers.Count());
        }

        #endregion
    }
}
