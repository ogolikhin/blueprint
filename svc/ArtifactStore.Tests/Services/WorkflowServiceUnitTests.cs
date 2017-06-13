using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services
{
    [TestClass]
    public class WorkflowServiceUnitTests
    {
        private Mock<IWorkflowRepository> _workflowRepository;
        private Mock<IArtifactVersionsRepository> _artifactVersionsRepository;
        private Mock<ISqlItemInfoRepository> _itemInfoRepository;
        private WorkflowService _workflowService;

        [TestInitialize]
        public void TestInitialize()
        {
            _workflowRepository = new Mock<IWorkflowRepository>(MockBehavior.Strict);
            _artifactVersionsRepository = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepository = new Mock<ISqlItemInfoRepository>(MockBehavior.Strict);
            _workflowService = new WorkflowService(_workflowRepository.Object, _artifactVersionsRepository.Object, _itemInfoRepository.Object);
        }

        [TestMethod]
        public async Task GetTransitionsAsync_GetsTransitions_SuccessfullyCallsRepo()
        {
            //Arrange
            var expected = new List<WorkflowTransition>()
            {
                 new WorkflowTransition()
            };
            _workflowRepository.Setup(t => t.GetTransitionsAsync(1, 1, 1, 1))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowService.GetTransitionsAsync(1, 1, 1, 1);

            //Assert
            _workflowRepository.Verify(t => t.GetTransitionsAsync(1, 1, 1, 1), Times.Exactly(1));
            Assert.AreEqual(QueryResultCode.Success, result.ResultCode);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(expected, result.Items);
        }

        [TestMethod]
        public async Task GetStateForArtifactAsync_GetState_SuccessfullyCallsGetStateForArtifactAsync()
        {
            //Arrange
            var expected = new WorkflowState()
            {
                Id = 10,
                WorkflowId = 4,
                Name = "Ready"
            };
            var revisionId = int.MaxValue;
            _itemInfoRepository.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);
            
            //Act
            var result = await _workflowService.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
            Assert.AreEqual(expected, result.Item);
            Assert.AreEqual(QueryResultCode.Success, result.ResultCode);
        }

        [TestMethod]
        public async Task GetStateForArtifactAsync_GetStateNullState_ReturnsFailedCode()
        {
            //Arrange
            var expected = (WorkflowState)null;
            var revisionId = int.MaxValue;
            _itemInfoRepository.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowService.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
            Assert.AreEqual(expected, result.Item);
            Assert.AreEqual(QueryResultCode.Failure, result.ResultCode);
        }

        [TestMethod]
        public async Task GetStateForArtifactAsync_GetStateStateWithUnsupportedWorkflowId_ReturnsFailedCode()
        {
            //Arrange
            var expected = new WorkflowState()
            {
                Id = 10,
                WorkflowId = -4,
                Name = "Ready"
            };
            var revisionId = int.MaxValue;
            _itemInfoRepository.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowService.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
            Assert.AreEqual(null, result.Item);
            Assert.AreEqual(QueryResultCode.Failure, result.ResultCode);
        }

        [TestMethod]
        public async Task GetStateForArtifactAsync_GetStateStateWithUnsupportedStateId_ReturnsFailedCode()
        {
            //Arrange
            var expected = new WorkflowState()
            {
                Id = -10,
                WorkflowId = 4,
                Name = "Ready"
            };
            var revisionId = int.MaxValue;
            _itemInfoRepository.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowService.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
            Assert.AreEqual(null, result.Item);
            Assert.AreEqual(QueryResultCode.Failure, result.ResultCode);
        }

        [TestMethod]
        public async Task GetTransitionsAsync_ChangeStateForArtifactAsync_SuccessfullyCallsRepo()
        {
            //Arrange
            int itemId = 1;
            int userId = 1;
            int workflowId = 4;
            int fromStateId = 2;
            int toStateId = 5;

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = itemId,
                VersionCount = 10,
                LockedByUser = new UserGroup
                {
                    Id = userId
                }
            };
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(itemId))
                .ReturnsAsync(false);
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(itemId, null, 1))
                .ReturnsAsync(vcArtifactInfo);
            var wfStateChangeParam = new WorkflowStateChangeParameter
            {
                CurrentVersionId = 10,
                ToStateId = toStateId,
                FromStateId = fromStateId
            };
            
            var fromState = new WorkflowState
            {
                Id = fromStateId,
                WorkflowId = workflowId,
                Name = "Ready"
            };
            var toState = new WorkflowState
            {
                Id = toStateId,
                Name = "Close",
                WorkflowId = workflowId
            };
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(userId, itemId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            var transition = new WorkflowTransition()
            {
                FromState = fromState,
                Id = 10,
                ToState = toState,
                WorkflowId = workflowId,
                Name = "Ready to Closed"
            };
            _workflowRepository.Setup(
                t => t.GetTransitionForAssociatedStatesAsync(userId, itemId, workflowId, fromStateId, toStateId))
                .ReturnsAsync(transition);

            
            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(1, itemId, It.IsAny<WorkflowStateChangeParameterEx>()))
                .ReturnsAsync(toState);

            //Act
            var result = await _workflowService.ChangeStateForArtifactAsync(1, itemId, wfStateChangeParam);

            //Assert
            Assert.AreEqual(toState, result.Item);
            Assert.AreEqual(QueryResultCode.Success, result.ResultCode);
        }
    }
}
