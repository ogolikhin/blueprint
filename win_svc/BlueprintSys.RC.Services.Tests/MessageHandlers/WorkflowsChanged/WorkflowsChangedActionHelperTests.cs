using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.WorkflowsChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.WorkflowsChanged
{
    /// <summary>
    /// Tests for the WorkflowsChangedActionHelper
    /// </summary>
    [TestClass]
    public class WorkflowsChangedActionHelperTests
    {
        private WorkflowsChangedActionHelper _helper;
        private WorkflowsChangedMessage _message;
        private Mock<IWorkflowsChangedRepository> _repositoryMock;
        private Mock<IWorkflowMessagingProcessor> _workflowMessagingProcessorMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _helper = new WorkflowsChangedActionHelper();
            _message = new WorkflowsChangedMessage
            {
                WorkflowId = 1,
                RevisionId = 2,
                UserId = 3
            };
            _repositoryMock = new Mock<IWorkflowsChangedRepository>(MockBehavior.Strict);
            _workflowMessagingProcessorMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Strict);
            _tenantInformation = new TenantInformation();
        }

        [TestMethod]
        public async Task WorkflowsChangedActionHelper_SendsNoMessage_WhenNoAffectedArtifactsAreFound()
        {
            //arrange
            var affectedArtifacts = new List<int>();
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            //act
            var result = await _helper.HandleWorkflowsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task WorkflowsChangedActionHelper_SendsMessage_WhenAffectedArtifactIsFound()
        {
            //arrange
            var affectedArtifacts = new List<int>
            {
                1
            };
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandleWorkflowsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task WorkflowsChangedActionHelper_SendsMessage_WhenMultipleAffectedArtifactsAreFound()
        {
            //arrange
            var affectedArtifacts = new List<int>
            {
                1,
                2
            };
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandleWorkflowsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task WorkflowsChangedActionHelper_SendsMultipleMessages_WhenAffectedArtifactsCountExceedsMaximumBatchSize()
        {
            //arrange
            var affectedArtifacts = new List<int>();
            for (int i = 0; i < ArtifactsChangedMessageSender.MaximumArtifactBatchSize + 1; i++)
            {
                affectedArtifacts.Add(i);
            }
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            //act
            var result = await _helper.HandleWorkflowsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            //assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(2));
        }
    }
}
