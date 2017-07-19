using AdminStore.Models;
using AdminStore.Models.Workflow;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

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
            string xml = 
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
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null);

            // Test resulting Actions Serialization
            try
            {
                string xmlActions = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlActions);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result = null;
            }

            return;
        }

        /// <summary>
        /// Deserialize/Serialize XML Transition Events
        /// </summary>
        [TestMethod]
        public void DeserializeTransitions()
        {
            // Import XML Events
            string xml =
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
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null);

            // Test resulting Events Serialization
            try
            {
                string xmlTriggers = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlTriggers);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result = null;
            }
        }

        /// <summary>
        /// Deserialize/Serialize XML Property Change Events
        /// </summary>
        [TestMethod]
        public void DeserializePropertyChanges()
        {
            // Import XML Events
            string xml =
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
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null);

            // Test resulting Events Serialization
            try
            {
                string xmlTriggers = SerializationHelper.ToXml(result);
                Assert.IsNotNull(xmlTriggers);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                result = null;
            }
        }
    }

	[TestClass]
    public class WorkflowControllerTest
    {
        private Mock<IWorkflowService> _workflowServiceMock;
        private Mock<IServiceLogRepository> _logMock;
        private Mock<IPrivilegesRepository> _privilegesRepositoryMock;
        private Mock<IWorkflowRepository> _workflowRepositoryMock;

        private WorkflowController _controller;
        private const int SessionUserId = 1;
        private const int WorkflowId = 1;
        private const InstanceAdminPrivileges AllProjectDataPermissions = InstanceAdminPrivileges.AccessAllProjectData;

        [TestInitialize]
        public void Initialize()
        {
            _privilegesRepositoryMock = new Mock<IPrivilegesRepository>();
            _logMock = new Mock<IServiceLogRepository>();
            _workflowServiceMock = new Mock<IWorkflowService>();
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();

            var session = new Session { UserId = SessionUserId };
            _controller = new WorkflowController(_workflowRepositoryMock.Object, _workflowServiceMock.Object, _logMock.Object, _privilegesRepositoryMock.Object)
            {
                Request = new HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            _controller.Request.Properties[ServiceConstants.SessionProperty] = session;
            _controller.Request.RequestUri = new Uri("http://localhost");
        }

        #region GetWorkflow

        [TestMethod]
        public async Task GetWorkflow_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflow()
        {
            //arrange
            var workflow = new WorkflowDto{ Name = "Workflow1", Description = "DescriptionWorkflow1", Status = true };
            _workflowServiceMock.Setup(repo => repo.GetWorkflowDetailsAsync(It.IsAny<int>())).ReturnsAsync(workflow);
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result = await _controller.GetWorkflow(WorkflowId) as OkNegotiatedContentResult<WorkflowDto>;

            //assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task GetWorkflow_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            //act
            try
            {
                await _controller.GetWorkflow(WorkflowId);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region GetWorkflows

        [TestMethod]
        public async Task GetWorkflows_AllParamsAreCorrectAndPermissionsOk_ReturnWorkflows()
        {
            //arrange
            var workflows = new QueryResult<WorkflowDto>() { Total = 10, Items = new List<WorkflowDto>() { new WorkflowDto(), new WorkflowDto() } };
            var pagination = new Pagination() { Limit = 10, Offset = 0 };
            var sorting = new Sorting() { Order = SortOrder.Asc, Sort = "name" };

            _workflowRepositoryMock.Setup(w => w.GetWorkflows(It.IsAny<Pagination>(), It.IsAny<Sorting>(), It.IsAny<string>(), It.IsAny<Func<Sorting, string>>()))
                .ReturnsAsync(workflows);

            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result =
                await _controller.GetWorkflows(pagination, sorting) as OkNegotiatedContentResult<QueryResult<WorkflowDto>>;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(workflows, result.Content);
        }

        [TestMethod]
        public async Task GetWorkflows_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            //act
            try
            {
                await _controller.GetWorkflows(new Pagination(), new Sorting());
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region UpdateWorkflowStatus

        [TestMethod]
        public async Task UpdateWorkflowStatus_AllRequirementsSatisfied_ReturnOkResult()
        {
            // Arrange
            var workflowDto = new WorkflowDto { VersionId = 1, Status = true };
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            // Act
            var result = await _controller.UpdateWorkflowStatus(WorkflowId, workflowDto);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }
        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task UpdateWorkflowStatus_BodyIsNull_BadRequestResult()
        {
            //arrange
            _privilegesRepositoryMock
                .Setup(r => r.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(AllProjectDataPermissions);
            //act
            await _controller.UpdateWorkflowStatus(SessionUserId, null);

            // Assert
            // Exception
        }
        [TestMethod]
        public async Task UpdateWorkflowStatus_WorkflowWithInvalidPermissions_ForbiddenResult()
        {
            //arrange
            var workflowDto = new WorkflowDto { VersionId = 1, Status = true };
            Exception exception = null;
            _privilegesRepositoryMock
                .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
                .ReturnsAsync(InstanceAdminPrivileges.ViewProjects);

            //act
            try
            {
                await _controller.UpdateWorkflowStatus(SessionUserId, workflowDto);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            //assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(AuthorizationException));
        }

        #endregion

        #region DeleteWorkflows

        [TestMethod]
        public async Task DeleteWorkflows_AllParamsAreCorrectAndPermissionsOk_ReturnDeletedCount()
        {
            //arrange
            var response = 2;
            var scope = new OperationScope() { Ids = new List<int>() { 1, 2, 3 }, SelectAll = false };
            var search = string.Empty;

            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);
            _workflowServiceMock.Setup(w => w.DeleteWorkflows(It.IsAny<OperationScope>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(response);

            //act
            var result = await _controller.DeleteWorkflows(scope, search) as OkNegotiatedContentResult<DeleteResult>;

            //assert
            Assert.IsNotNull(result);
            Assert.AreEqual(response, result.Content.TotalDeleted);
        }

        [TestMethod]
        public async Task DeleteWorkflows_ScopeIsNull_ReturnBadRequest()
        {
            //arrange
            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result = await _controller.DeleteWorkflows(null) as BadRequestErrorMessageResult;


            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public async Task DeleteWorkflow_ScopeIsEmpty_OkResultDeletedZero()
        {
            //arrange
            var scope = new OperationScope();
            _privilegesRepositoryMock
               .Setup(t => t.GetInstanceAdminPrivilegesAsync(SessionUserId))
               .ReturnsAsync(InstanceAdminPrivileges.AccessAllProjectData);

            //act
            var result = await _controller.DeleteWorkflows(scope) as OkNegotiatedContentResult<DeleteResult>;

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<DeleteResult>));
        }

        #endregion
    }
}
