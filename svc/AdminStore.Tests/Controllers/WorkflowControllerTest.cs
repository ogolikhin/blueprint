using System;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using AdminStore.Helpers;
using AdminStore.Repositories;
using AdminStore.Repositories.Workflow;
using AdminStore.Services.Workflow;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    /// <summary>
    /// Test Triggers Container class
    /// </summary>
    [Serializable()]
    [XmlRoot("WorkflowTriggers")]
    [XmlType("WorkflowTriggers")]
    public class WorkflowTriggers
    {
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Triggers")]
        [XmlArrayItem("TransitionTrigger", typeof(IeTransitionTrigger))]
        [XmlArrayItem("PropertyChangeTrigger", typeof(IePropertyChangeTrigger))]
        public List<IeTrigger> Triggers { get; set; }
       
    }
    /// <summary>
    /// Test Actions Container class
    /// </summary>
    [Serializable()]
    [XmlRoot("WorkflowActions")]
    [XmlType("WorkflowActions")]
    public class WorkflowActions
    {        
        [SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For Xml serialization, the property sometimes needs to be null")]
        [XmlArray("Actions")]
        [XmlArrayItem("NotificationAction", typeof(IeNotificationAction))]
        [XmlArrayItem("PropertyChangeAction", typeof(IePropertyChangeAction))]
        [XmlArrayItem("GenerateAction", typeof(IeGenerateAction))]
        public List<IeBaseAction> Actions { get; set; }

        public bool IsValid()
        {
            bool result = true;
            foreach(var act in Actions)
            {
                result &= act.IsValid();
            }
            return result;
        }
    }

 
    [TestClass]
    public class XmlTriggersActionsTest
    {
        /// <summary>
        /// Deserialize/Serialize XML Actions
        /// </summary>
        [TestMethod]
        public void DeserializeActions()
        {

            string xml = 
                "<WorkflowActions>" +
                "<Actions>" +
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
                    "<PropertyChangeAction>" +
                        "<Name>Property Change</Name>" +
                        "<Description>Property Change Test</Description>" +
                        "<Group>Group Admin</Group>" +
                        "<User>User1111</User>" +
                        "<PropertyName>Standard Property</PropertyName>" +
                        "<PropertyValue>1111111111111-2222222222</PropertyValue>" +
                        "<PropertyValueType>Text</PropertyValueType>" +
                    "</PropertyChangeAction>" +
                    "<GenerateAction>" +
                        "<Name>Generate Action</Name>" +
                        "<Description>Generate Action Test</Description>" +
                        "<Childs>3</Childs>" +
                        "<ArtifactType>UserStory</ArtifactType>" +
                    "</GenerateAction>" +
                "</Actions>" +
                "</WorkflowActions>";

            // Test Deserialization of imported XML Actions
            WorkflowActions result = null;
            try
            {
                result = SerializationHelper.FromXml<WorkflowActions>(xml);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null && result.IsValid());

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
        /// Deserialize/Serialize XML Triggers
        /// </summary>
        [TestMethod]
        public void DeserializeTriggers()
        {

            string xml =
                "<WorkflowTriggers>" +
                    "<Triggers>" +
                        "<TransitionTrigger>" +
                            "<TriggerType>1</TriggerType>"+
                            "<Name>TestTransition</Name>" +
                            "<Description>Trigger Deserialization test</Description>" + 
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" + 
                            "<Actions></Actions>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</TransitionTrigger>" +
                        "<PropertyChangeTrigger>" +
                            "<TriggerType>2</TriggerType>" +
                            "<Name>TestPropChange</Name>" +
                            "<Description>PropChangeTrigger Deserialization test</Description>" +
                            "<FromState>Begin</FromState>" +
                            "<ToState>TheEnd</ToState>" +
                            "<Actions></Actions>" +
                            "<PermissionGroups></PermissionGroups>" +
                        "</PropertyChangeTrigger>" +
                    "</Triggers>" +
                "</WorkflowTriggers>";

            // Test Deserialization of imported XML Triggers
            WorkflowTriggers result = null;
            try
            {
                result = SerializationHelper.FromXml<WorkflowTriggers>(xml);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                result = null;
            }
            Assert.IsTrue(result != null);

            // Serialize test
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

        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new WorkflowController();

            // Assert
            Assert.IsInstanceOfType(controller._privilegesManager, typeof(PrivilegesManager));
        }

        #endregion

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
    }
}
