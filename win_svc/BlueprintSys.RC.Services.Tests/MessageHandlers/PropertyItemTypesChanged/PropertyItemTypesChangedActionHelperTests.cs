using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.PropertyItemTypesChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.PropertyItemTypesChanged
{
    /// <summary>
    /// Tests for the PropertyItemTypesChangedActionHelper
    /// </summary>
    [TestClass]
    public class PropertyItemTypesChangedActionHelperTests
    {
        private PropertyItemTypesChangedActionHelper _helper;
        private PropertyItemTypesChangedMessage _message;
        private Mock<IPropertyItemTypesChangedRepository> _repositoryMock;
        private Mock<IWorkflowMessagingProcessor> _workflowMessagingProcessorMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _helper = new PropertyItemTypesChangedActionHelper();
            _message = new PropertyItemTypesChangedMessage
            {
                IsStandard = true,
                ChangeType = PropertyItemTypeChangeType.ItemType,
                UserId = 1,
                RevisionId = 2
            };
            _repositoryMock = new Mock<IPropertyItemTypesChangedRepository>(MockBehavior.Strict);
            _workflowMessagingProcessorMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Strict);
            _tenantInformation = new TenantInformation();
        }

        [TestMethod]
        public async Task PropertyItemTypesChangedActionHelper_SendsNoMessage_WhenNoAffectedArtifactsAreFound()
        {
            //arrange
            _message.ItemTypeIds = new List<int>
            {
                1
            };
            var affectedArtifacts = new List<int>();
            _repositoryMock.Setup(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>())).ReturnsAsync(RevisionStatus.Committed);
            _repositoryMock.Setup(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            //act
            var result = await _helper.HandlePropertyItemTypesChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>()), Times.Once());
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task PropertyItemTypesChangedActionHelper_SendsMessage_WhenAffectedArtifactIsFound()
        {
            //arrange
            _message.ChangeType = PropertyItemTypeChangeType.PropertyType;
            _message.IsStandard = false;
            _message.PropertyTypeIds = new List<int>
            {
                1,
                2
            };
            var affectedArtifacts = new List<int>
            {
                1
            };
            _repositoryMock.Setup(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>())).ReturnsAsync(RevisionStatus.Committed);
            _repositoryMock.Setup(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandlePropertyItemTypesChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task PropertyItemTypesChangedActionHelper_SendsMessage_WhenMultipleAffectedArtifactsAreFound()
        {
            //arrange
            _message.ItemTypeIds = new List<int>
            {
                1,
                2
            };
            _message.PropertyTypeIds = new List<int>
            {
                3,
                4
            };
            var affectedArtifactsForItems = new List<int>
            {
                5,
                6
            };
            var affectedArtifactsForProperties = new List<int>
            {
                7,
                8
            };
            _repositoryMock.Setup(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>())).ReturnsAsync(RevisionStatus.Committed);
            _repositoryMock.Setup(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(affectedArtifactsForItems);
            _repositoryMock.Setup(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(affectedArtifactsForProperties);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandlePropertyItemTypesChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task PropertyItemTypesChangedActionHelper_SendsMultipleMessages_WhenAffectedArtifactsCountExceedsMaximumBatchSize()
        {
            //arrange
            _message.ItemTypeIds = new List<int>
            {
                1,
                2
            };
            var affectedArtifacts = new List<int>();
            for (int i = 0; i < ArtifactsChangedMessageSender.MaximumArtifactBatchSize * 3 + 1; i++)
            {
                affectedArtifacts.Add(i);
            }
            _repositoryMock.Setup(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>())).ReturnsAsync(RevisionStatus.Committed);
            _repositoryMock.Setup(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandlePropertyItemTypesChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.ValidateRevision(It.IsAny<int>(), It.IsAny<IBaseRepository>(), It.IsAny<ActionMessage>(), It.IsAny<TenantInformation>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForItemTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Once);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIdsForPropertyTypes(It.IsAny<IEnumerable<int>>(), It.IsAny<bool>(), It.IsAny<int>()), Times.Never);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(4));
        }
    }
}
