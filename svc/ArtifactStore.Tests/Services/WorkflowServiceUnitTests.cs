using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ArtifactStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Services
{
    [TestClass]
    public class WorkflowServiceUnitTests
    {
        private Mock<IWorkflowRepository> _workflowRepositoryMock;
        private Mock<IArtifactVersionsRepository> _artifactVersionsRepositoryMock;
        private Mock<ISqlItemInfoRepository> _itemInfoRepositoryMock;
        private WorkflowService _workflowServiceMock;
        private ISqlHelper _sqlHelperMock;
        private Mock<IVersionControlService> _versionControlServiceMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>(MockBehavior.Strict);
            _artifactVersionsRepositoryMock = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _itemInfoRepositoryMock = new Mock<ISqlItemInfoRepository>(MockBehavior.Strict);
            _sqlHelperMock = new SqlHelperMock();
            _versionControlServiceMock = new Mock<IVersionControlService>();
            _workflowServiceMock = new WorkflowService(_workflowRepositoryMock.Object, 
                _artifactVersionsRepositoryMock.Object, 
                _itemInfoRepositoryMock.Object, 
                _sqlHelperMock,
                _versionControlServiceMock.Object);
        }

        [TestMethod]
        public async Task GetTransitionsAsync_GetsTransitions_SuccessfullyCallsRepo()
        {
            //Arrange
            var expected = new List<WorkflowTransition>()
            {
                 new WorkflowTransition()
            };
            _workflowRepositoryMock.Setup(t => t.GetTransitionsAsync(1, 1, 1, 1))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowServiceMock.GetTransitionsAsync(1, 1, 1, 1);

            //Assert
            _workflowRepositoryMock.Verify(t => t.GetTransitionsAsync(1, 1, 1, 1), Times.Exactly(1));
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
            _itemInfoRepositoryMock.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepositoryMock.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);
            
            //Act
            var result = await _workflowServiceMock.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepositoryMock.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
            Assert.AreEqual(expected, result.Item);
            Assert.AreEqual(QueryResultCode.Success, result.ResultCode);
        }

        [TestMethod]
        public async Task GetStateForArtifactAsync_GetStateNullState_ReturnsFailedCode()
        {
            //Arrange
            var expected = (WorkflowState)null;
            var revisionId = int.MaxValue;
            _itemInfoRepositoryMock.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepositoryMock.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowServiceMock.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepositoryMock.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
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
            _itemInfoRepositoryMock.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepositoryMock.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowServiceMock.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepositoryMock.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
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
            _itemInfoRepositoryMock.Setup(t => t.GetRevisionId(1, 1, null, null))
                .ReturnsAsync(revisionId);
            _workflowRepositoryMock.Setup(t => t.GetStateForArtifactAsync(1, 1, revisionId, true))
                .ReturnsAsync(expected);

            //Act
            var result = await _workflowServiceMock.GetStateForArtifactAsync(1, 1);

            //Assert
            _workflowRepositoryMock.Verify(t => t.GetStateForArtifactAsync(1, 1, revisionId, true), Times.Exactly(1));
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
            _artifactVersionsRepositoryMock.Setup(t => t.IsItemDeleted(itemId))
                .ReturnsAsync(false);
            _artifactVersionsRepositoryMock.Setup(t => t.GetVersionControlArtifactInfoAsync(itemId, null, 1))
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
            _workflowRepositoryMock.Setup(t => t.GetStateForArtifactAsync(userId, itemId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            var transition = new WorkflowTransition()
            {
                FromState = fromState,
                Id = 10,
                ToState = toState,
                WorkflowId = workflowId,
                Name = "Ready to Closed"
            };
            _workflowRepositoryMock.Setup(
                t => t.GetTransitionForAssociatedStatesAsync(userId, itemId, workflowId, fromStateId, toStateId))
                .ReturnsAsync(transition);

            
            _workflowRepositoryMock.Setup(t => t.ChangeStateForArtifactAsync(1, itemId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync(toState);

            //Act
            var result = await _workflowServiceMock.ChangeStateForArtifactAsync(1, itemId, wfStateChangeParam);

            //Assert
            Assert.AreEqual(toState, result.Item);
            Assert.AreEqual(QueryResultCode.Success, result.ResultCode);
        }
    }
}
