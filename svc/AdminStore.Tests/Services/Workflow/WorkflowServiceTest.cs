using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow.Validation;
using AdminStore.Services.Workflow.Validation.Data;
using AdminStore.Services.Workflow.Validation.Xml;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ProjectMeta;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Webhooks;

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
        private Mock<IProjectMetaRepository> _projectMetaRepository;
        private Mock<IArtifactRepository> _artifactRepository;
        private WorkflowService _service;
        private SqlWorkflow _workflow;
        private List<SqlWorkflowArtifactTypes> _workflowArtifactTypes;
        private List<SqlState> _workflowStates;
        private List<SqlWorkflowEventData> _workflowEvents;
        private List<UserDto> _userItems;
        private List<IeTrigger> _triggers;
        private QueryResult<UserDto> _userQueryResult;
        private ProjectTypes _projectTypes;
        private const int SessionUserId = 1;
        private const int WorkflowId = 10;
        private const string Location = "200 200";
        private const DiagramPort FromPort = DiagramPort.Left;
        private const DiagramPort ToPort = DiagramPort.Right;
        private Mock<IWorkflowDataValidator> _workflowDataValidatorMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<ISendMessageExecutor> _sendMessageExecutorMock;
        private Mock<IWebhooksRepository> _webhooksRepositoryMock;

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
            _projectMetaRepository = new Mock<IProjectMetaRepository>();
            _artifactRepository = new Mock<IArtifactRepository>();
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>();
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>();
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>();
            _sendMessageExecutorMock = new Mock<ISendMessageExecutor>();
            _webhooksRepositoryMock = new Mock<IWebhooksRepository>();

            _service = new WorkflowService(_workflowRepositoryMock.Object,
                _workflowXmlValidatorMock.Object,
                _usersRepositoryMock.Object,
                _workflowValidationErrorBuilder.Object,
                _projectMetaRepository.Object,
                _triggerConverter.Object,
                null,
                null,
                _artifactRepository.Object,
                _applicationSettingsRepositoryMock.Object,
                _serviceLogRepositoryMock.Object,
                _sendMessageExecutorMock.Object,
                _webhooksRepositoryMock.Object);

            _workflowDataValidatorMock = new Mock<IWorkflowDataValidator>();
            _workflowDataValidatorMock
                .Setup(q => q.ValidateUpdateDataAsync(It.IsAny<IeWorkflow>(), It.IsAny<ProjectTypes>()))
                .ReturnsAsync(new WorkflowDataValidationResult());
            typeof(WorkflowService)
                .GetField("_workflowDataValidator", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(_service, _workflowDataValidatorMock.Object);

            _workflow = new SqlWorkflow
            {
                Name = "Workflow1",
                Description = "Workflow1Description"
            };
            _workflowArtifactTypes = new List<SqlWorkflowArtifactTypes>
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
            _workflowStates = new List<SqlState>
            {
                new SqlState
                {
                    VersionId = 1,
                    WorkflowStateId = 1,
                    Name = "New",
                    Default = true,
                    CanvasSettings = I18NHelper.FormatInvariant("<S><LN>{0}</LN></S>", Location)
                },
                new SqlState
                {
                    VersionId = 2,
                    WorkflowStateId = 2,
                    Name = "Active",
                    CanvasSettings = string.Empty
                }
            };
            _workflowEvents = new List<SqlWorkflowEventData>
            {
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "Transition1",
                    FromState = "New",
                    ToState = "Active",
                    FromStateId = 1,
                    ToStateId = 2,
                    Permissions = "<P S=\"0\"><G>1</G></P>",
                    CanvasSettings = I18NHelper.FormatInvariant("<S><PRT><FR>{0}</FR><TO>{1}</TO></PRT></S>",
                        (int)FromPort, (int)ToPort),
                    Type = 0,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>"
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "PropertyChange1",
                    Type = 1,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                    PropertyTypeId = 1
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "PropertyChange2",
                    Type = 1,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                    PropertyTypeId = 2
                },
                new SqlWorkflowEventData
                {
                    WorkflowId = 10,
                    Name = "NewArtifact1",
                    Type = 2,
                    Triggers =
                        "<TSR><TS><T><AEN><ES><E>test.com</E></ES><M>4QOTT0IR7W</M></AEN></T></TS></TSR>",
                },
            };
            _userItems = new List<UserDto>();
            _userQueryResult = new QueryResult<UserDto>();
            _projectTypes = new ProjectTypes();
            _projectTypes.PropertyTypes.Add(new PropertyType { Name = "Property1", Id = 1 });
            _projectTypes.PropertyTypes.Add(new PropertyType { Name = "Property2", Id = 2 });
            _triggers = new List<IeTrigger> { new IeTrigger() };
        }

        #region GetWorkflowDetailsAsync

        [TestMethod]
        public async Task GetWorkflow_WorkflowExists_ReturnWorkflow()
        {
            // arrange
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflow);
            _projectMetaRepository.Setup(metaRepo => metaRepo.GetStandardProjectTypesAsync())
                .ReturnsAsync(_projectTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowEvents);

            _triggerConverter.Setup(
                converter =>
                    converter.FromXmlModel(It.IsAny<XmlWorkflowEventTriggers>(), It.IsAny<WorkflowDataNameMaps>(),
                        It.IsAny<ISet<int>>(), It.IsAny<ISet<int>>())).Returns(_triggers);

            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowArtifactTypes);

            // act
            var workflowDetails = await _service.GetWorkflowDetailsAsync(WorkflowId);

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
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((SqlWorkflow)null);

            // act
            await _service.GetWorkflowDetailsAsync(WorkflowId);

            // Exception
        }

        #endregion

        #region UpdateWorkflowAsync

        [TestMethod]
        [ExpectedException(typeof(ResourceNotFoundException))]
        public async Task UpdateWorkflowAsync_WorkflowNotExistsInDb_ThrowsResourceNotFound()
        {
            // Arrange
            var updateSatus = new UpdateWorkflowDto() { VersionId = 1, Status = true };
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)null);
            // Act
            await _service.UpdateWorkflowAsync(updateSatus, WorkflowId, SessionUserId);
        }

        [TestMethod]
        public async Task UpdateWorkflowAsync_StatusChanges_SuccessfullySendsMessage()
        {
            // Arrange
            var transactionMock = new Mock<IDbTransaction>();
            var updateSatus = new UpdateWorkflowDto() { VersionId = 1, Status = true };
            _workflowRepositoryMock
                .Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync((SqlWorkflow)new SqlWorkflow() { Active = false });
            _workflowRepositoryMock.Setup(
                repo =>
                    repo.CreateRevisionInTransactionAsync(It.IsAny<IDbTransaction>(), It.IsAny<int>(),
                        It.IsAny<string>())).ReturnsAsync(1);
            _workflowRepositoryMock.Setup(repo => repo.RunInTransactionAsync(It.IsAny<Func<IDbTransaction, Task>>()))
                .Returns(Task.Run(() => { }))
                .Callback((Func<IDbTransaction, Task> action) =>
                {
                    action(transactionMock.Object);
                });

            // Act
            await _service.UpdateWorkflowAsync(updateSatus, WorkflowId, SessionUserId);

            // Assert
             _sendMessageExecutorMock.Verify(a => a.Execute(It.IsAny<IApplicationSettingsRepository>(), It.IsAny<IServiceLogRepository>(), It.IsAny<ActionMessage>(), It.IsAny<IDbTransaction>()), Times.Once);
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
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(existingWorkflow);
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
            await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.Xml);

            // Assert
            // Exception
        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_WorkflowForXmlExists_ReturnWorkflow()
        {
            // arrange
            _userItems.Add(new UserDto { Id = 1, Login = "user" });
            _userQueryResult.Items = _userItems;
            _userQueryResult.Total = 1;

            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null))
                .ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            _triggerConverter.Setup(
                converter =>
                    converter.FromXmlModel(It.IsAny<XmlWorkflowEventTriggers>(), It.IsAny<WorkflowDataNameMaps>(),
                        It.IsAny<ISet<int>>(), It.IsAny<ISet<int>>())).Returns(_triggers);

            // act
            var workflowExport = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.Xml);

            // assert
            Assert.IsNotNull(workflowExport);

            Assert.IsNull(workflowExport.States[0].Location);
            Assert.IsNull(workflowExport.TransitionEvents[0].PortPair);

            Assert.AreEqual(3, workflowExport.Projects.Count);
            Assert.AreEqual(2, workflowExport.States.Count);
            Assert.AreEqual(1, workflowExport.TransitionEvents.Count);
            Assert.AreEqual(2, workflowExport.PropertyChangeEvents.Count);
            Assert.AreEqual(1, workflowExport.NewArtifactEvents.Count);
        }

        [TestMethod]
        public async Task GetWorkflowExportAsync_WorkflowForXmlExists_ReturnEmptyWorkflow()
        {
            // arrange
            _workflow = new SqlWorkflow();
            _workflowArtifactTypes = new List<SqlWorkflowArtifactTypes>();
            _workflowStates = new List<SqlState>();
            _workflowEvents = new List<SqlWorkflowEventData>();
            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null))
                .ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            // act
            var workflowExport = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.Xml);

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
            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null))
                .ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            _triggerConverter.Setup(
                converter =>
                    converter.FromXmlModel(It.IsAny<XmlWorkflowEventTriggers>(), It.IsAny<WorkflowDataNameMaps>(),
                        It.IsAny<ISet<int>>(), It.IsAny<ISet<int>>())).Returns(_triggers);

            // act
            var workflowForCanvas = await _service.GetWorkflowExportAsync(WorkflowId, WorkflowMode.Canvas);

            // assert
            Assert.IsNotNull(workflowForCanvas);

            Assert.AreEqual(workflowForCanvas.States[0].Location, Location);
            Assert.AreEqual(workflowForCanvas.TransitionEvents[0].PortPair.FromPort, FromPort);
            Assert.AreEqual(workflowForCanvas.TransitionEvents[0].PortPair.ToPort, ToPort);

            Assert.AreEqual(3, workflowForCanvas.Projects.Count);
            Assert.AreEqual(2, workflowForCanvas.States.Count);
            Assert.AreEqual(1, workflowForCanvas.TransitionEvents.Count);
            Assert.AreEqual(2, workflowForCanvas.PropertyChangeEvents.Count);
            Assert.AreEqual(1, workflowForCanvas.NewArtifactEvents.Count);
        }

        #endregion

        #region GetWorkflowArtifactTypesProperties

        [TestMethod]
        public async Task GetWorkflowArtifactTypesProperties_ParametersAreCorrect_ReturnProperties()
        {
            // arrange
            var standardProperties = new List<PropertyType>
            {
                new PropertyType
                {
                    Id = 175,
                    Name = "Std-Choice-Required-AllowMultiple-DefaultValue"
                },
                new PropertyType
                {
                    Id = 171,
                    Name = "Std-Date-Required-Validated-Min-Max-HasDefault"
                }
            };

            var standardArtifactTypeIds = new HashSet<int>() { 1, 2, 3 };

            var result = (await _service.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds)).ToList();

            Assert.IsNotNull(result);
            Assert.AreEqual(standardProperties.Count, result.Count);

            var nameProperty = result.FirstOrDefault(x => x.Name == WorkflowConstants.PropertyNameName);
            Assert.IsNotNull(nameProperty);

            var descriptionProperty = result.FirstOrDefault(x => x.Name == WorkflowConstants.PropertyNameDescription);
            Assert.IsNotNull(descriptionProperty);
        }

        #endregion

        #region GetWorkflowDiagramAsync

        [TestMethod]
        public async Task GetWorkflowDiagramAsync_WorkflowExists_ReturnWorkflow()
        {
            // arrange
            _userRepositoryMock.Setup(repo => repo.GetUsersAsync(It.IsAny<Pagination>(), null, null, null))
                .ReturnsAsync(_userQueryResult);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflow);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowArtifactTypesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowArtifactTypes);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowStatesAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowStates);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(_workflowEvents);
            _projectMetaRepository.Setup(repo => repo.GetStandardProjectTypesAsync()).ReturnsAsync(_projectTypes);

            _triggerConverter.Setup(
                converter =>
                    converter.FromXmlModel(It.IsAny<XmlWorkflowEventTriggers>(), It.IsAny<WorkflowDataNameMaps>(),
                        It.IsAny<ISet<int>>(), It.IsAny<ISet<int>>())).Returns(_triggers);

            // act
            var workflow = await _service.GetWorkflowDiagramAsync(WorkflowId);

            // assert
            Assert.IsNotNull(workflow);

            Assert.AreEqual(workflow.NumberOfStates, 2);
            Assert.AreEqual(workflow.NumberOfActions, 4);

            Assert.AreEqual(3, workflow.Projects.Count());
            Assert.AreEqual(2, workflow.States.Count());
            Assert.AreEqual(1, workflow.TransitionEvents.Count());
            Assert.AreEqual(2, workflow.PropertyChangeEvents.Count());
            Assert.AreEqual(1, workflow.NewArtifactEvents.Count());
        }

        #endregion

    }
}
