using System.Collections.Generic;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.MessageHandlers.ArtifactsChanged;
using BlueprintSys.RC.Services.MessageHandlers.UsersGroupsChanged;
using BluePrintSys.Messaging.CrossCutting.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace BlueprintSys.RC.Services.Tests.MessageHandlers.UsersGroupsChanged
{
    /// <summary>
    /// Tests for the UsersGroupsChangedActionHelper
    /// </summary>
    [TestClass]
    public class UsersGroupsChangedActionHelperTests
    {
        private UsersGroupsChangedActionHelper _helper;
        private UsersGroupsChangedMessage _message;
        private Mock<IUsersGroupsChangedRepository> _repositoryMock;
        private Mock<IWorkflowMessagingProcessor> _workflowMessagingProcessorMock;
        private TenantInformation _tenantInformation;

        [TestInitialize]
        public void TestInitialize()
        {
            _helper = new UsersGroupsChangedActionHelper();
            _message = new UsersGroupsChangedMessage
            {
                ChangeType = UsersGroupsChangedType.Update,
                UserId = 1,
                RevisionId = 2
            };
            _repositoryMock = new Mock<IUsersGroupsChangedRepository>(MockBehavior.Strict);
            _workflowMessagingProcessorMock = new Mock<IWorkflowMessagingProcessor>(MockBehavior.Strict);
            _tenantInformation = new TenantInformation();
        }

        [TestMethod]
        public async Task UsersGroupsChangedActionHelper_SendsNoMessage_WhenChangeTypeIsCreate()
        {
            // arrange
            _message.ChangeType = UsersGroupsChangedType.Create;
            _message.UserIds = new List<int>
            {
                1
            };
            // act
            var result = await _helper.HandleUsersGroupsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            // assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Never);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task UsersGroupsChangedActionHelper_SendsNoMessage_WhenNoAffectedArtifactsAreFound()
        {
            // arrange
            _message.ChangeType = UsersGroupsChangedType.Update;
            _message.GroupIds = new List<int>
            {
                1
            };
            var affectedArtifacts = new List<int>();
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            // act
            var result = await _helper.HandleUsersGroupsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            // assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task UsersGroupsChangedActionHelper_SendsMessage_WhenAffectedArtifactIsFound()
        {
            // arrange
            _message.ChangeType = UsersGroupsChangedType.Update;
            _message.UserIds = new List<int>
            {
                1,
                2
            };
            var affectedArtifacts = new List<int>
            {
                1
            };
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            // act
            var result = await _helper.HandleUsersGroupsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            // assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task UsersGroupsChangedActionHelper_SendsMessage_WhenMultipleAffectedArtifactsAreFound()
        {
            // arrange
            _message.ChangeType = UsersGroupsChangedType.Delete;
            _message.GroupIds = new List<int>
            {
                1,
                2
            };
            var affectedArtifacts = new List<int>
            {
                1,
                2
            };
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            // act
            var result = await _helper.HandleUsersGroupsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            // assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Once);
        }

        [TestMethod]
        public async Task UsersGroupsChangedActionHelper_SendsMultipleMessages_WhenAffectedArtifactsCountExceedsMaximumBatchSize()
        {
            // arrange
            _message.ChangeType = UsersGroupsChangedType.Update;
            _message.UserIds = new List<int>
            {
                1,
                2
            };
            var affectedArtifacts = new List<int>();
            for (int i = 0; i < ArtifactsChangedMessageSender.MaximumArtifactBatchSize * 2 + 1; i++)
            {
                affectedArtifacts.Add(i);
            }
            _repositoryMock.Setup(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>())).ReturnsAsync(affectedArtifacts);
            _workflowMessagingProcessorMock.Setup(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>())).Returns(Task.FromResult(true));
            // act
            var result = await _helper.HandleUsersGroupsChangedAction(_tenantInformation, _message, _repositoryMock.Object, _workflowMessagingProcessorMock.Object);
            // assert
            Assert.IsTrue(result);
            _repositoryMock.Verify(m => m.GetAffectedArtifactIds(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<int>()), Times.Once);
            _workflowMessagingProcessorMock.Verify(m => m.SendMessageAsync(It.IsAny<string>(), It.IsAny<IWorkflowMessage>()), Times.Exactly(3));
        }
    }
}
