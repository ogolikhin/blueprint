using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowServiceTest
    {
        #region Vars

        private Mock<IWorkflowXmlValidator> _workflowXmlValidatorMock;
        private Mock<IWorkflowRepository> _workflowRepositoryMock;
        private Mock<IUserRepository> _userRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<IWorkflowValidationErrorBuilder> _workflowValidationErrorBuilder ;
        private Mock<ITriggerConverter> _triggerConverter;
        private Mock<ISqlProjectMetaRepository> _projectMetaRepository;
        private WorkflowService _service;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;
        private const InstanceAdminPrivileges AllProjectDataPermissions = InstanceAdminPrivileges.AccessAllProjectData;

        #endregion
        [TestInitialize]
        public void Initialize()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _workflowXmlValidatorMock = new Mock<IWorkflowXmlValidator>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _usersRepositoryMock = new Mock<IUsersRepository>();
            _workflowValidationErrorBuilder = new Mock<IWorkflowValidationErrorBuilder>();
            _triggerConverter = new Mock<ITriggerConverter>();
            _projectMetaRepository = new Mock<ISqlProjectMetaRepository>();

            _service = new WorkflowService(_workflowRepositoryMock.Object,
                _workflowXmlValidatorMock.Object,
                _userRepositoryMock.Object,
                _usersRepositoryMock.Object,
                _workflowValidationErrorBuilder.Object, 
                _projectMetaRepository.Object,
                _triggerConverter.Object, 
                null, 
                null);
        }

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            //arrange
            var workflowRepositoryMock = new Mock<IWorkflowRepository>();
            var workflowValidatorMock = new Mock<IWorkflowXmlValidator>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var workflowValidationErrorBuilder = new Mock<IWorkflowValidationErrorBuilder>();
            var workflowService = new WorkflowService(
                workflowRepositoryMock.Object, 
                workflowValidatorMock.Object,
                userRepositoryMock.Object,
                _usersRepositoryMock.Object,
                workflowValidationErrorBuilder.Object, 
                null, 
                null, 
                null, 
                null);
            var workflowId = 10;
            var workflow = new SqlWorkflow { Name = "Workflow1", Description = "Workflow1Description" };
            var workflowArtifactTypesAndProjects = new List<SqlWorkflowArtifactTypes>
            {
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact1",
                    ArtifactTypeId = 204
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                   ProjectPath = "Project1",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 205
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 2,
                    ProjectPath = "Project2",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 205
                }
            };

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(workflowArtifactTypesAndProjects);

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
            var workflowValidationErrorBuilder = new Mock<IWorkflowValidationErrorBuilder>();
            var workflowService = new WorkflowService(
                workflowRepositoryMock.Object, 
                workflowValidatorMock.Object,
                userRepositoryMock.Object,
                _usersRepositoryMock.Object,
                workflowValidationErrorBuilder.Object, 
                null, 
                null, 
                null, 
                null);
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
            var updateSatus = new StatusUpdate { VersionId = 1, Active = true };
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);
            // Act
                await _service.UpdateWorkflowStatusAsync(updateSatus, WorkflowId, SessionUserId);
            // Assert
            // Exception
        }
        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task UpdateWorkflowStatusAsync_WorkflowHasDifferentVersion_ReturnConflicErrorResult()
        {
            // Arrange
            var existingWorkflow = new SqlWorkflow { VersionId = 1, WorkflowId = 1 };
            var updateSatus = new StatusUpdate { VersionId = 2, Active = true };
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(existingWorkflow);
            // Act
                await _service.UpdateWorkflowStatusAsync(updateSatus, WorkflowId, SessionUserId);
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
            var workflowArtifactTypes = new List<SqlWorkflowArtifactTypes>
            {
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 1,
                    ArtifactTypeId = 204,
                    ArtifactTypeName = "Artifact1"
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 2,
                    ArtifactTypeId = 205,
                    ArtifactTypeName = "Artifact2"
                }
            };

            var workflowStates = new SqlState {Name = "new", Default = true };
            var workflowsList = new List<SqlState> { workflowStates };

            var workflowEvents = new List<SqlWorkflowEventData>
            {
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "FirsTrigger",
                    FromState = "new",
                    ToState = "Active",
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1,
                    Triggers = "<Triggers><Trigger><Name>Trigger 1</Name><EmailNotificationAction></EmailNotificationAction></Trigger></Triggers>"
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "second Trigger",
                    FromState = "Active",
                    Permissions = "<P S=\"0\"/>",
                    Type = 1,
                    Triggers = "<Triggers><Trigger><Name>Trigger 2</Name><EmailNotificationAction></EmailNotificationAction></Trigger></Triggers>"
                }
            };

            var items = new List<UserDto> { new UserDto { Id = 1, Login = "user" } };
            var users = new QueryResult<UserDto> { Total = 1, Items = items };

            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null)).ReturnsAsync(users);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(workflowsList);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(workflowArtifactTypes);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>())).ReturnsAsync(workflowEvents);

            //act
            var workflowExport = await _service.GetWorkflowExportAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowExport);
            Assert.AreEqual(2, workflowExport.Projects.Count);
            Assert.AreEqual(1, workflowExport.States.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException), "Workflow doesn't exist.")]
        public async Task GetWorkflowExportAsync_WorkflowDetailsError()
        {
            //arrange
            var workflowId = 10;
            SqlWorkflow workflow = null;
            
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            
            //act
            var workflowExport = await _service.GetWorkflowExportAsync(workflowId);

        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_EmptyWorkflow()
        {
            //arrange
            var workflowId = 10;
            var workflow = new SqlWorkflow();
            var artifactTypes = new List<SqlWorkflowArtifactTypes>();
            var states = new List<SqlState>();
            var users = new QueryResult<UserDto>();
            var events = new List<SqlWorkflowEventData>();

            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null)).ReturnsAsync(users);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(artifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(states);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>())).ReturnsAsync(events);


            //act
            var workflowExport = await _service.GetWorkflowExportAsync(workflowId);

            //assert
            Assert.IsNotNull(workflowExport);
            Assert.IsFalse(workflowExport.IsActive);
            Assert.IsTrue(workflowExport.Projects.IsEmpty());
            Assert.IsTrue(workflowExport.States.IsEmpty());
            Assert.IsTrue(workflowExport.TransitionEvents.IsEmpty());
            Assert.IsTrue(workflowExport.PropertyChangeEvents.IsEmpty());
            Assert.IsTrue(workflowExport.NewArtifactEvents.IsEmpty());
        }
        #endregion
    }
}
