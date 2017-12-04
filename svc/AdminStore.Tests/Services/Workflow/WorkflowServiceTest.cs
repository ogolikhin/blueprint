using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Enums;
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
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;

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
        private Mock<IWorkflowValidationErrorBuilder> _workflowValidationErrorBuilder;
        private Mock<ITriggerConverter> _triggerConverter;
        private Mock<ISqlProjectMetaRepository> _projectMetaRepository;
        private Mock<IArtifactRepository> _artifactRepository;
        private WorkflowService _service;
        private SqlWorkflow _workflow;
        private List<SqlWorkflowArtifactTypes> _workflowArtifactTypes;
        private List<SqlState> _workflowStates;
        private List<SqlWorkflowEventData> _workflowEvents;
        private List<UserDto> _userItems;
        private QueryResult<UserDto> _userQueryResult;
        private ProjectTypes _projectTypes;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;

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
            _artifactRepository = new Mock<IArtifactRepository>();

            _service = new WorkflowService(_workflowRepositoryMock.Object,
                _workflowXmlValidatorMock.Object,
                _usersRepositoryMock.Object,
                _workflowValidationErrorBuilder.Object,
                _projectMetaRepository.Object,
                _triggerConverter.Object,
                null,
                null,
                _artifactRepository.Object);

            _workflow = new SqlWorkflow();
            _workflowArtifactTypes = new List<SqlWorkflowArtifactTypes>();
            _workflowStates = new List<SqlState>();
            _workflowEvents = new List<SqlWorkflowEventData>();
            _userItems = new List<UserDto>();
            _userQueryResult = new QueryResult<UserDto>();
            _projectTypes = new ProjectTypes();
        }

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            // arrange
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
                    ProjectId = 2,
                    ProjectPath = "Project2",
                    ArtifactTypeName = "Artifact2",
                    ArtifactTypeId = 205
                },
                new SqlWorkflowArtifactTypes
                {
                    ProjectId = 3,
                    ProjectPath = "Project3",
                    ArtifactTypeName = "Artifact3",
                    ArtifactTypeId = 206
                }
            };

            var stateNew = new SqlState { Name = "New", Default = true, WorkflowStateId = 1 };
            var stateActive = new SqlState { Name = "Active", Default = true, WorkflowStateId = 2 };
            var states = new List<SqlState> { stateNew, stateActive };

            var workflowEvents = new List<SqlWorkflowEventData>
            {
                new SqlWorkflowEventData
                {
                    Type = 0,
                    FromState = "New",
                    FromStateId = 1,
                    ToState = "Active",
                    ToStateId = 2
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "First trigger",
                    FromState = "New",
                    FromStateId = 1,
                    ToState = "Active",
                    ToStateId = 2,
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    Type = 1,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                    PropertyTypeId = 1
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "Second Trigger",
                    FromState = "New",
                    FromStateId = 1,
                    ToState = "Active",
                    ToStateId = 2,
                    Permissions = "<P S=\"0\"/>",
                    Type = 1,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                    PropertyTypeId = 2
                },
                new SqlWorkflowEventData
                {
                    Type = 2,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                },
            };

            var projectTypes = new ProjectTypes();
            projectTypes.PropertyTypes.Add(new PropertyType { Name = "Property1", Id = 1 });
            projectTypes.PropertyTypes.Add(new PropertyType { Name = "Property2", Id = 2 });

            var trigger = new IeTrigger();
            var triggers = new List<IeTrigger> { trigger };

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _projectMetaRepository.Setup(metaRepo => metaRepo.GetStandardProjectTypesAsync()).ReturnsAsync(projectTypes);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(states);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(workflowEvents);

            _triggerConverter.Setup(
                converter =>
                    converter.FromXmlModel(It.IsAny<XmlWorkflowEventTriggers>(), It.IsAny<WorkflowDataNameMaps>(),
                        It.IsAny<ISet<int>>(), It.IsAny<ISet<int>>())).Returns(triggers);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(workflowArtifactTypesAndProjects);

            // act
            var workflowDetails = await _service.GetWorkflowDetailsAsync(workflowId);

            // assert
            Assert.IsNotNull(workflowDetails);
            Assert.AreEqual(3, workflowDetails.Projects.Count());
            Assert.AreEqual(3, workflowDetails.ArtifactTypes.Count());
            Assert.AreEqual(2, workflowDetails.NumberOfStates);
            Assert.AreEqual(4, workflowDetails.NumberOfActions);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task GetWorkflow_ThereIsNoSuchWorkflow_NotFoundResult()
        {
            // arrange
            var workflowId = 10;

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);

            // act
            await _service.GetWorkflowDetailsAsync(workflowId);

            // Exception
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
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);

            // Act
            await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.XmlExport);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_WorkflowExists_ReturnWorkflow()
        {
            // arrange
            _workflow.Name = "Workflow1";
            _workflow.Description = "Workflow1Description";

            _workflowArtifactTypes.Add(new SqlWorkflowArtifactTypes
            {
                ProjectId = 1,
                ArtifactTypeId = 204,
                ArtifactTypeName = "Artifact1"
            });
            _workflowArtifactTypes.Add(new SqlWorkflowArtifactTypes
            {
                ProjectId = 2,
                ArtifactTypeId = 205,
                ArtifactTypeName = "Artifact2"
            });

            var workflowState = new SqlState { Name = "new", Default = true };
            _workflowStates.Add(workflowState);

            _workflowEvents.Add(new SqlWorkflowEventData
            {
                WorkflowId = 10,
                Name = "First Trigger",
                FromState = "new",
                ToState = "Active",
                Permissions = "<P S=\"0\"><G>1</G></P>",
                Type = 1,
                Triggers = "<Triggers><Trigger><Name>Trigger 1</Name><EmailNotificationAction></EmailNotificationAction></Trigger></Triggers>"
            });

            _workflowEvents.Add(new SqlWorkflowEventData
            {
                WorkflowId = 10,
                Name = "Second Trigger",
                FromState = "Active",
                Permissions = "<P S=\"0\"/>",
                Type = 1,
                Triggers = "<Triggers><Trigger><Name>Trigger 2</Name><EmailNotificationAction></EmailNotificationAction></Trigger></Triggers>"
            });

            _userItems.Add(new UserDto { Id = 1, Login = "user" });
            _userQueryResult.Items = _userItems;
            _userQueryResult.Total = 1;

            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null)).ReturnsAsync(_userQueryResult);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(_workflow);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(_workflowStates);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(_workflowArtifactTypes);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>())).ReturnsAsync(_workflowEvents);

            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            // act
            var workflowExport = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.XmlExport);

            // assert
            Assert.IsNotNull(workflowExport);
            Assert.AreEqual(2, workflowExport.Projects.Count);
            Assert.AreEqual(1, workflowExport.States.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException), "Workflow doesn't exist.")]
        public async Task GetWorkflowExportAsync_WorkflowDetailsError()
        {
            // arrange
            SqlWorkflow workflow = null;

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            // act
            await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.XmlExport);

        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_EmptyWorkflow()
        {
            // arrange
            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null)).ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>())).ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            // act
            var workflowExport = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.XmlExport);

            // assert
            Assert.IsNotNull(workflowExport);
            Assert.IsFalse(workflowExport.IsActive);
            Assert.IsTrue(workflowExport.Projects.IsEmpty());
            Assert.IsTrue(workflowExport.States.IsEmpty());
            Assert.IsTrue(workflowExport.TransitionEvents.IsEmpty());
            Assert.IsTrue(workflowExport.PropertyChangeEvents.IsEmpty());
            Assert.IsTrue(workflowExport.NewArtifactEvents.IsEmpty());
        }


        [TestMethod]
        public async Task GetWorkflowExportAsync_WorkflowForCanvasExists_ReturnWorkflow()
        {
            // arrange
            var location = "200 200";
            var workflowStateFrom = new SqlState { VersionId = 1, WorkflowStateId = 1, Name = "New", Default = true, CanvasSettings = $"<S><LN>{location}</LN></S>" };
            var workflowStateTo = new SqlState { VersionId = 2, WorkflowStateId = 2, Name = "Active", Default = true, CanvasSettings = String.Empty };
            _workflowStates.Add(workflowStateFrom);
            _workflowStates.Add(workflowStateTo);

            var fromPort = DiagramPort.Left;
            var toPort = DiagramPort.Right;
            var eventData = new SqlWorkflowEventData
            {
                WorkflowId = 10,
                Name = "First Trigger",
                FromState = "New",
                ToState = "Active",
                FromStateId = 1,
                ToStateId = 2,
                Permissions = "<P S=\"0\"><G>1</G></P>",
                CanvasSettings = $"<S><PRT><FR>{(int)fromPort}</FR><TO>{(int)toPort}</TO></PRT></S>",
                Type = 0,
                Triggers =
                    "<Triggers><Trigger><Name>Trigger 1</Name><EmailNotificationAction></EmailNotificationAction></Trigger></Triggers>"
            };

            _workflowEvents.Add(eventData);

            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null)).ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>())).ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>())).ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>())).ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            // act
            var workflowForCanvas = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.Canvas);

            // assert
            Assert.IsNotNull(workflowForCanvas);

            Assert.IsNotNull(workflowForCanvas.States);
            Assert.AreEqual(workflowForCanvas.States.Count, 2);
            Assert.AreEqual(workflowForCanvas.States[0].Location, location);

            Assert.IsNotNull(workflowForCanvas.TransitionEvents);
            Assert.AreEqual(workflowForCanvas.TransitionEvents.Count, 1);
            Assert.AreEqual(workflowForCanvas.TransitionEvents[0].PortPair.FromPort, fromPort);
            Assert.AreEqual(workflowForCanvas.TransitionEvents[0].PortPair.ToPort, toPort);
        }
        #endregion

        #region GetWorkflowArtifactTypesProperties

        [TestMethod]
        public async Task GetWorkflowArtifactTypesProperties_ParametersAreCorrect_ReturnProperties()
        {
            // arrange

            var standardProperties = new List<PropertyType>()
             {
                new PropertyType()
                {
                    Id = 175,
                    Name = "Std-Choice-Required-AllowMultiple-DefaultValue"
                },
                new PropertyType()
                {
                    Id = 171,
                    Name = "Std-Date-Required-Validated-Min-Max-HasDefault"
                }
             };

            var standardArtifactTypeIds = new HashSet<int>() { 1, 2, 3 };
            _artifactRepository.Setup(repo => repo.GetStandardProperties(standardArtifactTypeIds))
                .ReturnsAsync(standardProperties);

            var result = (await _service.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds)).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            var nameProperty = result.FirstOrDefault(x => x.Name == WorkflowConstants.PropertyNameName);
            Assert.IsNotNull(nameProperty);

            var descriptionProperty = result.FirstOrDefault(x => x.Name == WorkflowConstants.PropertyNameDescription);
            Assert.IsNotNull(descriptionProperty);
        }

        #endregion
    }
}
