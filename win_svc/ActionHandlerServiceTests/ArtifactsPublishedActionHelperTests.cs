using System.Collections.Generic;
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
        private Mock<IArtifactsPublishedRepository> _repositoryMock;
        private TenantInformation _tenantInformation;
        private List<SqlWorkflowEvent> _triggers;
        private List<SqlWorkFlowStateInformation> _states;
        private List<SqlProject> _projects;
        private List<NotificationAction> _notificationActions;
        private ArtifactsPublishedMessage _messageWithModifiedProperties;
        private Dictionary<int, List<int>> _instancePropertyTypeIds;
        private const int WorkflowStateId = 10;
        private const int PropertyTypeId = 20;

        [TestInitialize]
        public void TestInitialize()
        {
            _repositoryMock = new Mock<IArtifactsPublishedRepository>();
            _tenantInformation = new TenantInformation {ConnectionString = "", Id = "", Settings = ""};
            _triggers = new List<SqlWorkflowEvent> {new SqlWorkflowEvent {CurrentStateId = 0, VersionItemId = 0, EventPropertyTypeId = 0, EventType = 0, HolderId = 0, RequiredNewStateId = 0, RequiredPreviousStateId = 0, Triggers = "", WorkflowId = 0}, new SqlWorkflowEvent {CurrentStateId = 1, VersionItemId = 0, EventPropertyTypeId = 0, EventType = 0, HolderId = 0, RequiredNewStateId = 0, RequiredPreviousStateId = 0, Triggers = "", WorkflowId = 0}};
            _states = new List<SqlWorkFlowStateInformation> {new SqlWorkFlowStateInformation {WorkflowStateId = WorkflowStateId, ArtifactId = 0, EndRevision = 0, ItemId = 0, ItemTypeId = 0, LockedByUserId = 0, Name = "", ProjectId = 0, Result = 0, StartRevision = 0, WorkflowId = 0, WorkflowName = "", WorkflowStateName = ""}};
            _projects = new List<SqlProject> {new SqlProject {ItemId = 0, Name = ""}};
            _instancePropertyTypeIds = new Dictionary<int, List<int>> {{0, new List<int> {0}}};
            _notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = WorkflowStateId, PropertyTypeId = PropertyTypeId, ToEmails = new [] {""}}};
            _messageWithModifiedProperties = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation {ModifiedProperties = new List<PublishedPropertyInformation> {new PublishedPropertyInformation {TypeId = PropertyTypeId}}}}};
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoTriggersAreFound()
        {
            var message = new ArtifactsPublishedMessage();

            //empty list of triggers
            var emptyTriggersList = new List<SqlWorkflowEvent>();
            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(emptyTriggersList);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoPublishedArtifactsAreFound()
        {
            //empty array of artifacts
            var message = new ArtifactsPublishedMessage {Artifacts = new PublishedArtifactInformation[] { }};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoWorkflowStatesAreFound()
        {
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            //empty list of states
            var emptyStatesList = new List<SqlWorkFlowStateInformation>();
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(emptyStatesList);

            var actionHelper = new ArtifactsPublishedActionHelper();

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenNoNotificationActionsAreFound()
        {
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(_states);
            _repositoryMock.Setup(m => m.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_projects);

            var actionsParserMock = new Mock<IActionsParser>();
            //empty list of notification actions
            var emptyNotificationActionsList = new List<NotificationAction>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(emptyNotificationActionsList);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenThereIsATriggerWithoutModifiedProperties()
        {
            //no modified properties
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation()}};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(_states);
            _repositoryMock.Setup(m => m.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_projects);

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(_notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenThereIsATriggerWithModifiedProperties()
        {
            //message with a list of modified properties
            var message = _messageWithModifiedProperties;

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(_states);
            _repositoryMock.Setup(m => m.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_projects);
            _repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_instancePropertyTypeIds);

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(_notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenPropertyTypeIdDoesNotMatchNotificationPropertyTypeId()
        {
            //non-matching Property Type IDs
            const int notificationPropertyTypeId = 1 + PropertyTypeId;
            var notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = WorkflowStateId, PropertyTypeId = notificationPropertyTypeId, ToEmails = new[] {""}}};
            var message = new ArtifactsPublishedMessage {Artifacts = new[] {new PublishedArtifactInformation {ModifiedProperties = new List<PublishedPropertyInformation> {new PublishedPropertyInformation {TypeId = PropertyTypeId}}}}};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(_states);
            _repositoryMock.Setup(m => m.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_projects);
            _repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_instancePropertyTypeIds);
            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, message, _repositoryMock.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ArtifactsPublishedActionHelper_ReturnsTrue_WhenWorkflowStateIdDoesNotMatchConditionalStateId()
        {
            //non-matching State IDs
            const int conditionalStateId = 1 + WorkflowStateId;
            var states = new List<SqlWorkFlowStateInformation> {new SqlWorkFlowStateInformation {WorkflowStateId = WorkflowStateId, ArtifactId = 0, EndRevision = 0, ItemId = 0, ItemTypeId = 0, LockedByUserId = 0, Name = "", ProjectId = 0, Result = 0, StartRevision = 0, WorkflowId = 0, WorkflowName = "", WorkflowStateName = ""}};
            var notificationActions = new List<NotificationAction> {new NotificationAction {ConditionalStateId = conditionalStateId, PropertyTypeId = PropertyTypeId, ToEmails = new[] {""}}};

            _repositoryMock.Setup(m => m.GetWorkflowPropertyTransitionsForArtifactsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<int>>())).ReturnsAsync(_triggers);
            _repositoryMock.Setup(m => m.GetWorkflowStatesForArtifactsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(states);
            _repositoryMock.Setup(m => m.GetProjectNameByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_projects);
            _repositoryMock.Setup(m => m.GetInstancePropertyTypeIdsMap(It.IsAny<IEnumerable<int>>())).ReturnsAsync(_instancePropertyTypeIds);

            var actionsParserMock = new Mock<IActionsParser>();
            actionsParserMock.Setup(m => m.GetNotificationActions(It.IsAny<IEnumerable<SqlWorkflowEvent>>())).Returns(notificationActions);
            var actionHelper = new ArtifactsPublishedActionHelper(actionsParserMock.Object, new Mock<INServiceBusServer>().Object);

            var result = await actionHelper.HandleAction(_tenantInformation, _messageWithModifiedProperties, _repositoryMock.Object);
            Assert.IsTrue(result);
        }
    }
}
