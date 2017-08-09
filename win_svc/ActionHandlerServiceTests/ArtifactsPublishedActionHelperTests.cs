using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActionHandlerService;
using ActionHandlerService.Helpers;
using ActionHandlerService.MessageHandlers.ArtifactPublished;
using ActionHandlerService.Models;
using ActionHandlerService.Repositories;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace ActionHandlerServiceTests
{
    /// <summary>
    /// Tests for the Artifacts Published Action Helper in the Action Handler Service
    /// </summary>
    [TestClass]
    public class ArtifactsPublishedActionHelperTests
    {
        private TenantInformation _tenantInformation;
        private Task<List<SqlArtifactTriggers>> _triggers;
        private Task<List<SqlWorkFlowStateInformation>> _states;
        private List<NotificationAction> _notificationActions;
        private ArtifactsPublishedMessage _messageWithModifiedProperties;
        private const int WorkflowStateId = 10;
        private const int PropertyTypeId = 20;

        [TestInitialize]
        public void TestInitialize()
        {
            _tenantInformation = new TenantInformation {ConnectionString = "", Id = "", Settings = ""};
            _triggers = Task.FromResult(new List<SqlArtifactTriggers> {new SqlArtifactTriggers {CurrentStateId = 0, EventPropertyTypeId = 0, EventType = 0, HolderId = 0, RequiredNewStateId = 0, RequiredPreviousStateId = 0, Triggers = "", WorkflowId = 0}, new SqlArtifactTriggers {CurrentStateId = 1, EventPropertyTypeId = 0, EventType = 0, HolderId = 0, RequiredNewStateId = 0, RequiredPreviousStateId = 0, Triggers = "", WorkflowId = 0}});
            _states = Task.FromResult(new List<SqlWorkFlowStateInformation> {new SqlWorkFlowStateInformation {WorkflowStateId = WorkflowStateId, ArtifactId = 0, EndRevision = 0, ItemId = 0, ItemTypeId = 0, LockedByUserId = 0, Name = "", ProjectId = 0, Result = 0, StartRevision = 0, WorkflowId = 0, WorkflowName = "", WorkflowStateName = ""}});
            _notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = WorkflowStateId, MessageTemplate = "", PropertyTypeId = PropertyTypeId, ToEmail = ""}};
            _messageWithModifiedProperties = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation {ModifiedProperties = new List<PublishedPropertyInformation> {new PublishedPropertyInformation {TypeId = PropertyTypeId}}}}};
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoTriggersAreFound()
        {
            var message = new ArtifactsPublishedMessage();

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            //empty list of triggers
            var emptyTriggersList = Task.FromResult(new List<SqlArtifactTriggers>());
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(emptyTriggersList);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoPublishedArtifactsAreFound()
        {
            //empty array of artifacts
            var message = new ArtifactsPublishedMessage {Artifacts = new PublishedArtifactInformation[] { }};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoWorkflowStatesAreFound()
        {
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            //empty list of states
            var emptyStatesList = Task.FromResult(new List<SqlWorkFlowStateInformation>());
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(emptyStatesList);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoNotificationActionsAreFound()
        {
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_states);

            var actionsParserMock = new Mock<IActionsParser>();
            //empty list of notification actions
            var emptyNotificationActionsList = new List<NotificationAction>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlArtifactTriggers>>())).Returns(emptyNotificationActionsList);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenThereIsATriggerWithoutModifiedProperties()
        {
            //no modified properties
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_states);

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlArtifactTriggers>>())).Returns(_notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenThereIsATriggerWithModifiedProperties()
        {
            //message with a list of modified properties
            var message = _messageWithModifiedProperties;

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_states);
            repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, List<int>>());

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlArtifactTriggers>>())).Returns(_notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenPropertyTypeIdDoesNotMatchNotificationPropertyTypeId()
        {
            //non-matching Property Type IDs
            const int notificationPropertyTypeId = 1 + PropertyTypeId;
            var notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = WorkflowStateId, MessageTemplate = "", PropertyTypeId = notificationPropertyTypeId, ToEmail = ""}};
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation {ModifiedProperties = new List<PublishedPropertyInformation> {new PublishedPropertyInformation {TypeId = PropertyTypeId}}}}};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(_states);
            repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new Dictionary<int, List<int>>());
            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlArtifactTriggers>>())).Returns(notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenWorkflowStateIdDoesNotMatchConditionalStateId()
        {
            //non-matching State IDs
            const int conditionalStateId = 1 + WorkflowStateId;
            var states = Task.FromResult(new List<SqlWorkFlowStateInformation> {new SqlWorkFlowStateInformation {WorkflowStateId = WorkflowStateId, ArtifactId = 0, EndRevision = 0, ItemId = 0, ItemTypeId = 0, LockedByUserId = 0, Name = "", ProjectId = 0, Result = 0, StartRevision = 0, WorkflowId = 0, WorkflowName = "", WorkflowStateName = ""}});
            var notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = conditionalStateId, MessageTemplate = "", PropertyTypeId = PropertyTypeId, ToEmail = ""}};

            var repositoryMock = new Mock<IActionHandlerServiceRepository>();
            repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).Returns(_triggers);
            repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(states);
            repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>()))
               .ReturnsAsync(new Dictionary<int, List<int>>());

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlArtifactTriggers>>())).Returns(notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, _messageWithModifiedProperties, repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ActionsParser_ReturnsNoActions_WhenTriggerStringIsEmpty()
        {
            const string triggerString = "";
            var sqlArtifactTriggers = new[] {new SqlArtifactTriggers {Triggers = triggerString}};
            var actionsParser = new ActionsParser();
            var notificationActions = actionsParser.GetNotificationActions(sqlArtifactTriggers);
            Assert.AreEqual(0, notificationActions.Count());
        }

        [TestMethod]
        public void ActionsParser_ReturnsActions_WhenTriggersExist()
        {
            const string triggerString = "this should be a valid trigger string";
            var sqlArtifactTriggers = new[] {new SqlArtifactTriggers {Triggers = triggerString}};
            var actionsParser = new ActionsParser();
            var notificationActions = actionsParser.GetNotificationActions(sqlArtifactTriggers);
            Assert.IsTrue(notificationActions.Any());
        }
    }
}
