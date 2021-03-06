﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Xml.Schema;
using AdminStore.Models;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.DTO;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [TestClass]
    public class XmlTriggersActionsTest
    {
        /// <summary>
        /// Deserialize/Serialize XML Actions
        /// </summary>
        [TestMethod]
        public void DeserializeActions()
        {
            // Import XML Actions content
            var xml =
                "<Transition>" +
                    "<Triggers>" +
                        "<Trigger>" +
                        "<NotificationAction>" +
                            "<Name>NotificationAction</Name>" +
                            "<Description>Notification Test</Description>" +
                            "<Groups>" +
                                "<Group>Group111</Group>" +
                                "<Group>Group222</Group>" +
                            "</Groups>" +
                            "<Users>" +
                                "<User>User111</User>" +
                                "<User>User222</User>" +
                            "</Users>" +
                            "<Emails>" +
                                "<Email>user1@mail.com</Email>" +
                                "<Email>user2@mail.com</Email>" +
                            "</Emails>" +
                            "<PropertyTarget>Property Name</PropertyTarget>" +
                            "<Message>Property was changed</Message>" +
                        "</NotificationAction>" +
                    "</Trigger>" +
                    "<Trigger>" +
                        "<PropertyChangeAction>" +
                            "<Name>Property Change</Name>" +
                            "<Description>Property Change Test</Description>" +
                            "<Group>Group Admin</Group>" +
                            "<User>User1111</User>" +
                            "<PropertyName>Standard Property</PropertyName>" +
                            "<PropertyValue>1111111111111-2222222222</PropertyValue>" +
                            "<PropertyValueType>Text</PropertyValueType>" +
                        "</PropertyChangeAction>" +
                    "</Trigger>" +
                    "<Trigger>" +
                        "<GenerateAction>" +
                            "<Name>Generate Action</Name>" +
                            "<Description>Generate Action Test</Description>" +
                            "<Childs>3</Childs>" +
                            "<ArtifactType>UserStory</ArtifactType>" +
                        "</GenerateAction>" +
                        "</Trigger>" +
                    "</Triggers>" +
                "</Transition>";

            // Test Deserialization of imported XML Actions
            IeTransitionEvent result = null;

            try
            {
                result = SerializationHelper.FromXml<IeTransitionEvent>(xml);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result != null);

            // Test resulting Actions Serialization
            try
            {
                var xmlActions = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlActions);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Deserialize/Serialize XML Transition Events
        /// </summary>
        [TestMethod]
        public void DeserializeTransitions()
        {
            // Import XML Events
            var xml =
                "<Workflow>" +
                    "<Transitions>" +
                        "<Transition>" +
                            "<EventType>Transition</EventType>" +
                            "<Name>TestTransition</Name>" +
                            "<Description>Trigger Deserialization test</Description>" +
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" +
                            "<Triggers></Triggers>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</Transition>" +
                    "</Transitions>" +
                  "</Workflow>";

            // Test Deserialization of imported XML Events
            IeWorkflow result = null;

            try
            {
                result = SerializationHelper.FromXml<IeWorkflow>(xml);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result != null);

            // Test resulting Events Serialization
            try
            {
                var xmlTriggers = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlTriggers);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Deserialize/Serialize XML Property Change Events
        /// </summary>
        [TestMethod]
        public void DeserializePropertyChanges()
        {
            // Import XML Events
            var xml =
                "<Workflow>" +
                    "<PropertyChanges>" +
                        "<PropertyChange>" +
                            "<EventType>PropertyChange</EventType>" +
                            "<Name>TestPropChange</Name>" +
                            "<Description>PropChangeTrigger Deserialization test</Description>" +
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" +
                            "<Triggers></Triggers>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</PropertyChange>" +
                    "</PropertyChanges>" +
                  "</Workflow>";

            // Test Deserialization of imported XML Events
            IeWorkflow result = null;

            try
            {
                result = SerializationHelper.FromXml<IeWorkflow>(xml);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }

            Assert.IsTrue(result != null);

            // Test resulting Events Serialization
            try
            {
                var xmlTriggers = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlTriggers);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }

    [TestClass]
    public class WorkflowControllerTests
    {
        private Mock<IWorkflowService> _workflowServiceMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IPrivilegesRepository> _privilegesRepositoryMock;
        private Mock<IWorkflowRepository> _workflowRepositoryMock;

        private WorkflowController _controller;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;
        private CopyWorkflowDto _copyWorkfloDto;
        private const InstanceAdminPrivileges AllProjectDataPermissions = InstanceAdminPrivileges.AccessAllProjectData;
        private const int FolderId = 1;
        private const int ProjectId = 1;

        private QueryResult<WorkflowProjectArtifactTypesDto> _expectedArtifacts;
        private Pagination _pagination;
        private List<WorkflowProjectSearch> _projects;
        private string _search = "Project";
        private OperationScope _scope;
        private DWorkflow _dWorkflow;

        [TestInitialize]
        public void Initialize()
        {
            _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _workflowServiceMock = new Mock<IWorkflowService>();
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();

            var session = new Session { UserId = SessionUserId };
            _controller = new WorkflowController(_workflowRepositoryMock.Object, _workflowServiceMock.Object, _logMock.Object,
                _privilegesRepositoryMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");

            _expectedArtifacts = new QueryResult<WorkflowProjectArtifactTypesDto>
            {
                Items = new List<WorkflowProjectArtifactTypesDto>
                {
                    new WorkflowProjectArtifactTypesDto
                    {
                        ProjectId = 1,
                        ProjectName = "TestProject",
                        Artifacts = new List<WorkflowArtifactType>
                        {
                            new WorkflowArtifactType
                            {
                                Id = 1,
                                Name = "TestArtifact"
                            }
                        }
                    }
                },
                Total = 1
            };

            _pagination = new Pagination { Limit = int.MaxValue, Offset = 0 };

            _projects = new List<WorkflowProjectSearch>
            {
                new WorkflowProjectSearch
                {
                    ItemId = 1,
                    Name = "Project1",
                    Path = "Path1"
                },
                new WorkflowProjectSearch
                {
                    ItemId = 2,
                    Name = "Project2",
                    Path = "Path2"
                }
            };

            _scope = new OperationScope { Ids = new List<int> { 1, 2, 3 }, SelectAll = false };

            _copyWorkfloDto = new CopyWorkflowDto { Name = "TestWorkflow" };

            _dWorkflow = new DWorkflow();
        }

        #region AssignProjectsAndArtifactTypesToWorkflow
        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_AllParamsAreCorrectAndPermissionsOk_ReturnListInstanceItem()
        {
            // arrange
            var scope = new WorkflowAssignScope
            {
                AllArtifacts = true,
                AllProjects = true,
                ArtifactIds = new List<int>(),
                ProjectIds = new List<int>(),
            };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowRepositoryMock.Setup(q => q.AssignProjectsAndArtifactTypesToWorkflow(WorkflowId, new WorkflowAssignScope())).ReturnsAsync(new AssignProjectsResult { TotalAssigned = 2, AllProjectsAssignedToWorkflow = false });

            // act
            var result = await _controller.AssignProjectsAndArtifactTypesToWorkflow(WorkflowId, scope);

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_InvalidPermission_ReturnAuthorizationException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);

            // act
            try
            {
                await _controller.AssignProjectsAndArtifactTypesToWorkflow(WorkflowId, new WorkflowAssignScope());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task AssignProjectsAndArtifactsToWorkflow_InvalidWorkflowAssignScope_ReturnBadRequestException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            try
            {
                await _controller.AssignProjectsAndArtifactTypesToWorkflow(WorkflowId, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
        }
        #endregion

        #region AssignArtifactTypesToProjectInWorkflow
        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_AllParamsAreCorrectAndPermissionsOk_ReturnSyncResult()
        {
            // arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            var expectedResult = new SyncResult { TotalAdded = 2, TotalDeleted = 1 };

            _workflowRepositoryMock.Setup(q => q.AssignArtifactTypesToProjectInWorkflow(WorkflowId, ProjectId, _scope)).ReturnsAsync(new SyncResult { TotalAdded = 2, TotalDeleted = 1 });

            // act
            var result = await _controller.AssignArtifactTypesToProjectInWorkflow(WorkflowId, ProjectId, _scope) as OkNegotiatedContentResult<SyncResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedResult.TotalAdded, result.Content.TotalAdded);
            Assert.AreEqual(expectedResult.TotalDeleted, result.Content.TotalDeleted);
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_InvalidPermission_ReturnAuthorizationException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);

            // act
            try
            {
                await _controller.AssignArtifactTypesToProjectInWorkflow(WorkflowId, ProjectId, _scope);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task AssignArtifactTypesToProjectInWorkflow_ArtifactTypesIdsIsNull_ReturnBadRequestException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            try
            {
                await _controller.AssignArtifactTypesToProjectInWorkflow(WorkflowId, ProjectId, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
        }

        #endregion

        #region GetWorkflow

        [TestMethod]
        public async Task GetWorkflow_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflow()
        {
            // arrange
            var workflow = new WorkflowDetailsDto { Name = "Workflow1", Description = "DescriptionWorkflow1", Active = true };
            _workflowServiceMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            var result = await _controller.GetWorkflow(WorkflowId) as OkNegotiatedContentResult<WorkflowDetailsDto>;

            // assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetWorkflow_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            // arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.GetWorkflow(WorkflowId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region GetWorkflows

        [TestMethod]
        public async Task GetWorkflows_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflows()
        {
            // arrange
            var workflows = new QueryResult<WorkflowDto> { Total = 10, Items = new List<WorkflowDto> { new WorkflowDto(), new WorkflowDto() } };
            var pagination = new Pagination { Limit = 10, Offset = 0 };
            var sorting = new Sorting { Order = SortOrder.Asc, Sort = "name" };

            _workflowRepositoryMock.Setup(w => w.GetWorkflows(It.IsAny<Pagination>(), It.IsAny<Sorting>(), It.IsAny<string>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(workflows);

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            var result =
                await _controller.GetWorkflows(pagination, sorting) as OkNegotiatedContentResult<QueryResult<WorkflowDto>>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(workflows, result.Content);
        }

        [TestMethod]
        public async Task GetWorkflows_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            // arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.GetWorkflows(new Pagination { Limit = 10, Offset = 0 }, new Sorting());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region UpdateStatus

        [TestMethod]
        public async Task UpdateStatus_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            var updateSatus = new StatusUpdate { VersionId = 1, Active = true };
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            // Act
            var result = await _controller.UpdateStatus(WorkflowId, updateSatus);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<int>));
        }
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateStatus_BodyIsNull_BadRequestResult()
        {
            // arrange
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            // act
            await _controller.UpdateStatus(SessionUserId, null);

            // Assert
            // Exception
        }
        [TestMethod]
        public async Task UpdateStatus_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            // arrange
            var updateSatus = new StatusUpdate { VersionId = 1, Active = true };
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.UpdateStatus(SessionUserId, updateSatus);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region UnassignProjectsAndArtifactTypesFromWorkflowAsync

        [TestMethod]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_AllParamsAreCorrectAndPermissionsOk_ReturnDeletedCount()
        {
            // arrange
            var response = 3;
            var scope = new OperationScope { Ids = new List<int> { 1, 2, 3 }, SelectAll = false };
            var search = string.Empty;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            _workflowRepositoryMock.Setup(w => w.UnassignProjectsAndArtifactTypesFromWorkflowAsync(WorkflowId, scope, search))
                .ReturnsAsync(response);

            // act
            var result = await _controller.UnassignProjectsAndArtifactTypesFromWorkflowAsync(WorkflowId, scope, search) as OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response, result.Content.TotalDeleted);
        }

        [TestMethod]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_ScopeIsEmpty_ReturnDeletedCount()
        {
            // arrange
            var response = 0;
            var scope = new OperationScope { Ids = new List<int>(), SelectAll = false };
            var search = string.Empty;

            _privilegesRepositoryMock
                                    .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                                    .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            var result = await _controller.UnassignProjectsAndArtifactTypesFromWorkflowAsync(WorkflowId, scope, search) as OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response, result.Content.TotalDeleted);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_ScopeIsNull_ReturnDeletedCount()
        {
            // Arrange
            OperationScope scope = null;
            var search = string.Empty;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // Act
            var result = await _controller.UnassignProjectsAndArtifactTypesFromWorkflowAsync(WorkflowId, scope, search) as OkNegotiatedContentResult<DeleteResult>;
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UnassignProjectsAndArtifactTypesFromWorkflowAsync_UserDoesNotHaveRequiredPermissions_ForbiddenResult()
        {
            // arrange
            var search = string.Empty;
            var scope = new OperationScope { SelectAll = false, Ids = new List<int> { 2, 3 } };
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.None);

            // act
            await _controller.UnassignProjectsAndArtifactTypesFromWorkflowAsync(WorkflowId, scope, search);
        }

        #endregion

        #region DeleteWorkflows

        [TestMethod]
        public async Task DeleteWorkflows_AllParamsAreCorrectAndPermissionsOk_ReturnDeletedCount()
        {
            // arrange
            var response = 2;
            var scope = new OperationScope { Ids = new List<int> { 1, 2, 3 }, SelectAll = false };
            var search = string.Empty;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            _workflowServiceMock.Setup(w => w.DeleteWorkflows(It.IsAny<OperationScope>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(response);

            // act
            var result = await _controller.DeleteWorkflows(scope, search) as OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response, result.Content.TotalDeleted);
        }

        [TestMethod]
        public async Task DeleteWorkflows_ScopeIsNull_ReturnBadRequest()
        {
            // arrange
            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            var result = await _controller.DeleteWorkflows(null) as BadRequestErrorMessageResult;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task DeleteWorkflow_ScopeIsEmpty_OkResultDeletedZero()
        {
            // arrange
            var scope = new OperationScope();
            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            var result = await _controller.DeleteWorkflows(scope) as OkNegotiatedContentResult<DeleteResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<DeleteResult>));
        }

        #endregion

        #region SearchProjectsByName

        [TestMethod]
        public async Task SearchProjectsByName_AllParamsAreCorrectAndPermissionsOk_ReturnProjects()
        {
            // arrange
            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowRepositoryMock.Setup(w => w.SearchProjectsByName(WorkflowId, _search))
                .ReturnsAsync(_projects);

            // act
            var result = await _controller.SearchProjectsByName(WorkflowId, _search) as
                    OkNegotiatedContentResult<IEnumerable<WorkflowProjectSearch>>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);

            var project1 = result.Content.ToList().FirstOrDefault(x => x.ItemId == _projects[0].ItemId);
            var project2 = result.Content.ToList().FirstOrDefault(x => x.ItemId == _projects[1].ItemId);

            Assert.IsNotNull(project1);
            Assert.IsNotNull(project2);

            Assert.AreEqual(_projects.Count, result.Content.Count());
        }

        [TestMethod]
        public async Task SearchProjectsByName_IsufficientPermissions_ReturnAuthorizationException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);

            _workflowRepositoryMock.Setup(w => w.SearchProjectsByName(WorkflowId, _search))
                .ReturnsAsync(_projects);

            // act
            try
            {
                await _controller.SearchProjectsByName(WorkflowId, _search);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region ExportWorkflow

        [TestMethod]
        public async Task ExportWorkflow_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            // arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.ExportWorkflow(WorkflowId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task ExportWorkflow_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            var workflow = new IeWorkflow
            {
                Name = "Workflow1",
                Description = "DescriptionWorkflow1",
                States = new List<IeState>(),
                Projects = new List<IeProject>()
            };
            _workflowServiceMock.Setup(repo => repo.GetWorkflowExportAsync(It.IsAny<int>(), It.IsAny<WorkflowMode>())).ReturnsAsync(workflow);
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);

            // Act
            var result = await _controller.ExportWorkflow(WorkflowId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ResponseMessageResult));
        }

        #endregion

        #region ValidateWorkflowXmlAgainstXsd_Success

        // A failure of this test means that the IeWorkflow model has changed
        // and regenerating of the xml schema IeWorkflow.xsd is required, see below:
        // xsd.exe AdminStore.dll /t:IeWorkflow
        // Note that an extending of the IeWorkflow will not fail the test.
        [TestMethod]
        public void ValidateWorkflowXmlAgainstXsd_Success()
        {
            // Arrange
            var workflow = GetTestWorkflow();

            var xml = SerializationHelper.ToXml(workflow);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            // Act
            WorkflowController.ValidateWorkflowXmlAgainstXsd(stream);
        }

        [TestMethod]
        [ExpectedException(typeof(XmlSchemaValidationException))]
        public void ValidateWorkflowXmlAgainstXsd_Failure()
        {
            // Arrange
            var workflow = GetTestWorkflow();

            var xml = SerializationHelper.ToXml(workflow);
            xml = xml.Replace("<Workflow Id=\"99\">", "<Workflow Id=\"99\"><aaa>bbb</aaa>");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            // Act
            WorkflowController.ValidateWorkflowXmlAgainstXsd(stream);
        }

        private static IeWorkflow GetTestWorkflow()
        {
            var workflow = new IeWorkflow
            {
                Id = 99,
                Name = "name",
                Description = "description",
                States = new List<IeState>
                {
                    new IeState
                    {
                        Id = 11,
                        Name = "name1",
                        IsInitial = true
                    },
                    new IeState
                    {
                        Id = 11,
                        Name = "name2",
                        IsInitial = false
                    }
                },
                TransitionEvents = new List<IeTransitionEvent>
                {
                    new IeTransitionEvent
                    {
                        Id = 22,
                        Name = "name",
                        FromState = "aaa",
                        FromStateId = 33,
                        ToState = "bbb",
                        ToStateId = 44,
                        SkipPermissionGroups = true,
                        Triggers = new List<IeTrigger>
                        {
                            new IeTrigger
                            {
                                Name = "name",
                                Condition = new IeStateCondition
                                {
                                    State = "ccc"
                                },
                                Action = new IeEmailNotificationAction
                                {
                                    Name = "name",
                                    PropertyName = "property name",
                                    PropertyId = 11,
                                    Emails = new List<string> { "aaa", "bbb" },
                                    Message = "message"
                                }
                            }
                        },
                        PermissionGroups = new List<IeGroup>
                        {
                            new IeGroup
                            {
                                Id = 11,
                                Name = "name"
                            }
                        }
                    }
                },
                PropertyChangeEvents = new List<IePropertyChangeEvent>
                {
                    new IePropertyChangeEvent
                    {
                        Id = 11,
                        Name = "name",
                        PropertyName = "property name",
                        PropertyId = 11,
                        Triggers = new List<IeTrigger>
                        {
                            new IeTrigger
                            {
                                Name = "name1",
                                Action = new IeGenerateAction
                                {
                                    Name = "name 2",
                                    GenerateActionType = GenerateActionTypes.Children,
                                    ArtifactType = "artifact type",
                                    ArtifactTypeId = 11,
                                    ChildCount = 3
                                }
                            },
                            new IeTrigger
                            {
                                Name = "name3",
                                Action = new IeGenerateAction
                                {
                                    Name = "name 4",
                                    GenerateActionType = GenerateActionTypes.TestCases
                                }
                            },
                            new IeTrigger
                            {
                                Name = "name5",
                                Action = new IeGenerateAction
                                {
                                    Name = "name 6",
                                    GenerateActionType = GenerateActionTypes.UserStories
                                }
                            }
                        }
                    }
                },
                NewArtifactEvents = new List<IeNewArtifactEvent>
                {
                    new IeNewArtifactEvent
                    {
                        Id = 11,
                        Name = "name1",
                        Triggers = new List<IeTrigger>
                        {
                            new IeTrigger
                            {
                                Name = "name",
                                Action = new IePropertyChangeAction
                                {
                                    Name = "name2",
                                    PropertyName = "property name",
                                    PropertyValue = "property value",
                                    PropertyId = 11,
                                    ValidValues = new List<IeValidValue>
                                    {
                                        new IeValidValue
                                        {
                                            Id = 22,
                                            Value = "value1"
                                        },
                                        new IeValidValue
                                        {
                                            Id = 33,
                                            Value = "value2"
                                        }
                                    },
                                    UsersGroups = new IeUsersGroups
                                    {
                                        UsersGroups = new List<IeUserGroup>
                                        {
                                            new IeUserGroup
                                            {
                                                Id = 11,
                                                Name = "user",
                                                IsGroup = false
                                            },
                                            new IeUserGroup
                                            {
                                                Id = 22,
                                                Name = "group",
                                                IsGroup = true
                                            }
                                        },
                                        IncludeCurrentUser = true
                                    }
                                }
                            }
                        }
                    }
                },
                Projects = new List<IeProject>
                {
                    new IeProject
                    {
                        Id = 11,
                        Path = "path",
                        ArtifactTypes = new List<IeArtifactType>
                        {
                            new IeArtifactType
                            {
                                Id = 11,
                                Name = "name"
                            }
                        }
                    }
                }
            };

            return workflow;
        }

        #endregion

        #region CreateWorkflow

        [TestMethod]
        public async Task CreateWorkflow_InvalidPermissions_ForbiddenResult()
        {

            // arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.CreateWorkflow(new CreateWorkflowDto());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task CreateWorkflow_CreateModelIsEmpty_BadRequest()
        {
            // arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            try
            {
                await _controller.CreateWorkflow(null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorMessages.CreateWorkfloModelIsEmpty, exception.Message);
        }

        [TestMethod]
        public async Task CreateWorkflow_ModelIsValid_ReturnSuccessResult()
        {
            // arrange

            var model = new CreateWorkflowDto { Name = "some unique name", Description = "some description" };
            var returnId = 1;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            _workflowServiceMock.Setup(s => s.CreateWorkflow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(returnId);

            // act
            var result = await _controller.CreateWorkflow(model);

            // assert
            Assert.IsNotNull(result.Content);
            Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
        }

        #endregion

        #region GetWorkflowAvailableProjects
        [TestMethod]
        public async Task GetWorkflowAvailableProjects_AllParamsAreCorrectAndPermissionsOk_ReturnListInstanceItem()
        {
            // arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowRepositoryMock.Setup(q => q.GetWorkflowAvailableProjectsAsync(WorkflowId, FolderId)).ReturnsAsync(new List<InstanceItem>());

            // act
            var actualResult = await _controller.GetWorkflowAvailableProjects(WorkflowId, FolderId);

            // assert
            Assert.IsNotNull(actualResult);
        }

        [TestMethod]
        public async Task GetWorkflowAvailableProjects_InvalidPermission_ReturnAuthorizationException()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AssignAdminRoles);

            _workflowRepositoryMock.Setup(q => q.GetWorkflowAvailableProjectsAsync(WorkflowId, FolderId)).ReturnsAsync(new List<InstanceItem>());

            // act
            try
            {
                await _controller.GetWorkflowAvailableProjects(WorkflowId, FolderId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }
        #endregion

        #region GetProjectArtifactTypesAssignedToWorkflowAsync

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_SuccessfulGettingArtifacts_OkResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);



            _workflowRepositoryMock.Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, string.Empty))
                                   .ReturnsAsync(_expectedArtifacts);

            // act
            var actualResult =
                await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId,
                                                                            _pagination,
                                                                            string.Empty)
                                                                            as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_expectedArtifacts, actualResult.Content);
            Assert.AreEqual(_expectedArtifacts.Total, actualResult.Content.Total);
            Assert.AreEqual(_expectedArtifacts.Items.Count(), actualResult.Content.Items.Count());
        }

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_PaginationIsNull_ReturnBadRequestResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _pagination = null;

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, string.Empty))
                .ReturnsAsync(_expectedArtifacts);

            BadRequestException exception = null;

            // Act
            try
            {
                var actualResult = await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, string.Empty) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.InvalidPagination);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_OffsetIsNull_ReturnBadRequestResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _pagination.Offset = null;

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, string.Empty))
                .ReturnsAsync(_expectedArtifacts);

            BadRequestException exception = null;

            // Act
            try
            {
                var actualResult = await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, string.Empty) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.IncorrectOffsetParameter);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_LimitIsNull_ReturnBadRequestResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _pagination.Limit = null;

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, string.Empty))
                .ReturnsAsync(_expectedArtifacts);

            BadRequestException exception = null;

            // Act
            try
            {
                var actualResult = await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, string.Empty) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.IncorrectLimitParameter);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_SearchExceedsLimit_ReturnBadRequestResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // should be <= 250
            var search =
                "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium q";

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, search))
                 .ReturnsAsync(_expectedArtifacts);


            BadRequestException exception = null;

            // Act
            try
            {
                var actualResult = await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, search) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;
            }
            catch (BadRequestException ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNotNull(exception);
            Assert.AreEqual(exception.Message, ErrorMessages.SearchFieldLimitation);
            Assert.AreEqual(exception.ErrorCode, ErrorCodes.BadRequest);
        }

        [TestMethod]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_SearchIsInLimit_SuccessfulGettingArtifacts_OkResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // 250 characters - OK
            string search =

                "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium.";

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, search))
                .ReturnsAsync(_expectedArtifacts);

            // Act
            var actualResult =
                await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, search) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;

            // Exception

            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_expectedArtifacts, actualResult.Content);
            Assert.AreEqual(_expectedArtifacts.Total, actualResult.Content.Total);
            Assert.AreEqual(_expectedArtifacts.Items.Count(), actualResult.Content.Items.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetProjectArtifactTypesAssignedtoWorkflowAsync_IncorrectUserPermissions_ReturnForbiddenErrorResult()
        {
            // Arrange
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewUsers);

            _workflowRepositoryMock
                .Setup(q => q.GetProjectArtifactTypesAssignedtoWorkflowAsync(WorkflowId, _pagination, string.Empty))
                .ReturnsAsync(_expectedArtifacts);

            // Act
            var actualResult = await _controller.GetProjectArtifactTypesAssignedToWorkflowAsync(WorkflowId, _pagination, string.Empty) as OkNegotiatedContentResult<QueryResult<WorkflowProjectArtifactTypesDto>>;
        }

        #endregion

        #region UpdateWorkflow

        [TestMethod]
        public async Task UpdateWorkflow_AllParametersAreFine_ReturnSuccessResult()
        {
            // arrange
            var updateWorkflowDto = new UpdateWorkflowDto
            {
                Description = "Some text",
                Name = "Some name",
                Status = true
            };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            _workflowServiceMock.Setup(s => s.UpdateWorkflowAsync(It.IsAny<UpdateWorkflowDto>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(1));

            // act
            var result = await _controller.UpdateWorkflow(WorkflowId, updateWorkflowDto);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.NoContent);
            Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
        }

        [TestMethod]
        public async Task UpdateWorkflow_UserDoesNotHaveEnoughPrivileges_ReturnForbidden()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            // act
            try
            {
                await _controller.UpdateWorkflow(WorkflowId, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task UpdateWorkflow_WorkflowDtoIsNull_ReturnWorkflowModelIsEmpty()
        {
            // arrange
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            // act
            try
            {
                await _controller.UpdateWorkflow(WorkflowId, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(exception.Message, ErrorMessages.WorkflowModelIsEmpty);
        }

        #endregion

        #region CopyWorkflowAsync

        [TestMethod]
        public async Task CopyWorkflowAsync_AllParamsAreCorrectAndPermissionsOk_WorkflowSuccessfullyUpdated()
        {
            // arrange
            var updatedWorkflowId = 1;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowRepositoryMock.Setup(w => w.CopyWorkflowAsync(WorkflowId, SessionUserId, _copyWorkfloDto))
                .ReturnsAsync(updatedWorkflowId);

            // act
            var result = await _controller.CopyWorkflowAsync(WorkflowId, _copyWorkfloDto) as OkNegotiatedContentResult<int>;

            // assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Content);
            Assert.AreEqual(result.Content, updatedWorkflowId);
        }

        [TestMethod]
        public async Task CopyWorkflowAsync_InSufficientPermissions_ReturnAuthorizationException()
        {
            // arrange
            var updatedWorkflowId = 1;
            Exception exception = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);

            _workflowRepositoryMock.Setup(w => w.CopyWorkflowAsync(WorkflowId, SessionUserId, _copyWorkfloDto))
                .ReturnsAsync(updatedWorkflowId);

            // act
            try
            {
                await _controller.CopyWorkflowAsync(WorkflowId, _copyWorkfloDto);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        [TestMethod]
        public async Task CopyWorkflowAsync_WrokflowModelIncorrect_ReturnBadRequestException()
        {
            // arrange
            var updatedWorkflowId = 0;
            Exception exception = null;
            _copyWorkfloDto = null;

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowRepositoryMock
                .Setup(w => w.CopyWorkflowAsync(WorkflowId, SessionUserId, _copyWorkfloDto))
                .ReturnsAsync(updatedWorkflowId);

            // act
            try
            {
                await _controller.CopyWorkflowAsync(WorkflowId, _copyWorkfloDto);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(exception.Message, ErrorMessages.WorkflowModelIsEmpty);
        }

        #endregion

        #region GetWorkflowDiagram

        [TestMethod]
        public async Task GetWorkflowDiagram_AllParamsAreCorrectAndPermissionsOk_WorkflowSuccessfullyReceived()
        {
            // arrange
            var workflow = new DWorkflow
            {
                Name = "Workflow1",
                Description = "DescriptionWorkflow1",
                States = new List<DState>(),
                Projects = new List<DProject>()
            };

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowServiceMock.Setup(repo => repo.GetWorkflowDiagramAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            // act
            var result = await _controller.GetWorkflowDiagram(WorkflowId) as OkNegotiatedContentResult<DWorkflow>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content, workflow);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetWorkflowDiagram_InSufficientPermissions_ReturnAuthorizationException()
        {
            // arrange
            var workflow = new DWorkflow();

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);

            _workflowServiceMock.Setup(repo => repo.GetWorkflowDiagramAsync(It.IsAny<int>())).ReturnsAsync(workflow);

            // act
            await _controller.GetWorkflowDiagram(WorkflowId);
        }

        #endregion

        #region GetWorkflowArtifactTypesProperties

        [TestMethod]
        public async Task GetWorkflowArtifactTypesProperties_AllParamsAreCorrectAndPermissionsOk_PropertiesSuccessfullyReturned()
        {
            var properties = new List<PropertyType>
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
                },
                new PropertyType
                {
                    Id = WorkflowConstants.PropertyTypeFakeIdName,
                    Name = WorkflowConstants.PropertyNameName
                },
                new PropertyType
                {
                    Id = WorkflowConstants.PropertyTypeFakeIdDescription,
                    Name = WorkflowConstants.PropertyNameDescription
                },
            };

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            var standardArtifactTypeIds = new HashSet<int> { 1, 2, 3 };

            _workflowServiceMock.Setup(service => service.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds)).ReturnsAsync(properties);

            var result = await _controller.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task GetWorkflowArtifactTypesProperties_InSufficientPermissions_ReturnAuthorizationException()
        {
            // arrange
            var standardArtifactTypeIds = new HashSet<int> { 1, 2, 3 };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);

            _workflowServiceMock
                .Setup(service => service.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds))
                .ReturnsAsync(new List<PropertyType>());

            // act
            await _controller.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task GetWorkflowArtifactTypesProperties_StandardArtifactTypeIdsIsNull_ReturnAuthorizationException()
        {
            // arrange
            ISet<int> standardArtifactTypeIds = null;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowServiceMock
                .Setup(service => service.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds))
                .ReturnsAsync(new List<PropertyType>());

            // act
            await _controller.GetWorkflowArtifactTypesProperties(standardArtifactTypeIds);
        }

        #endregion

        #region UpdateWorkflowDiagram

        [TestMethod]
        public async Task UpdateWorkflowDiagram_AllParamsAreCorrectAndPermissionsOk_ImportWorkflowResultSuccessfullyReceived()
        {
            // arrange
            var expectedUpdateWorkflowDiagramResult = new ImportWorkflowResult
            {
                ResultCode = ImportWorkflowResultCodes.Ok
            };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowServiceMock
                .Setup(repo => repo.UpdateWorkflowViaImport(It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<IeWorkflow>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.Is<WorkflowMode>(mode => mode == WorkflowMode.Canvas)))
                .ReturnsAsync(expectedUpdateWorkflowDiagramResult);

            // act
            var result = await _controller.UpdateWorkflowDiagram(WorkflowId, _dWorkflow) as OkNegotiatedContentResult<ImportWorkflowResult>;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedUpdateWorkflowDiagramResult, result.Content);
            Assert.AreEqual(expectedUpdateWorkflowDiagramResult.ResultCode, result.Content.ResultCode);
        }

        [TestMethod]
        public async Task UpdateWorkflowDiagram_InvalidModel_ReturnBadRequestResult()
        {
            // arrange
            var expectedUpdateWorkflowDiagramResult = new ImportWorkflowResult
            {
                ResultCode = ImportWorkflowResultCodes.InvalidModel
            };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowServiceMock
                .Setup(repo => repo.UpdateWorkflowViaImport(It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<IeWorkflow>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.Is<WorkflowMode>(mode => mode == WorkflowMode.Canvas)))
                .ReturnsAsync(expectedUpdateWorkflowDiagramResult);

            // act
            var result = await _controller.UpdateWorkflowDiagram(WorkflowId, _dWorkflow) as ResponseMessageResult;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.Response.StatusCode);

            var content = await result.Response.Content.ReadAsAsync<ImportWorkflowResult>();
            Assert.AreEqual(expectedUpdateWorkflowDiagramResult.ResultCode, content.ResultCode);
        }

        [TestMethod]
        public async Task UpdateWorkflowDiagram_Conflict_ReturnConflictResult()
        {
            // arrange
            var expectedUpdateWorkflowDiagramResult = new ImportWorkflowResult
            {
                ResultCode = ImportWorkflowResultCodes.Conflict
            };

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            _workflowServiceMock
                .Setup(repo => repo.UpdateWorkflowViaImport(It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<IeWorkflow>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.Is<WorkflowMode>(mode => mode == WorkflowMode.Canvas)))
                .ReturnsAsync(expectedUpdateWorkflowDiagramResult);

            // act
            var result = await _controller.UpdateWorkflowDiagram(WorkflowId, _dWorkflow) as ResponseMessageResult;

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.Conflict, result.Response.StatusCode);

            var content = await result.Response.Content.ReadAsAsync<ImportWorkflowResult>();
            Assert.AreEqual(expectedUpdateWorkflowDiagramResult.ResultCode, content.ResultCode);
        }

        [TestMethod]
        [ExpectedException(typeof(AuthorizationException))]
        public async Task UpdateWorkflowDiagram_InSufficientPermissions_ReturnAuthorizationException()
        {
            // arrange
            var updateWorkflowDiagramResult = new ImportWorkflowResult();

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewGroups);

            _workflowServiceMock
                .Setup(repo => repo.UpdateWorkflowViaImport(It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<IeWorkflow>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.Is<WorkflowMode>(mode => mode == WorkflowMode.Canvas)))
                .ReturnsAsync(updateWorkflowDiagramResult);

            // act
            await _controller.UpdateWorkflowDiagram(WorkflowId, _dWorkflow);
        }

        #endregion
    }
}
