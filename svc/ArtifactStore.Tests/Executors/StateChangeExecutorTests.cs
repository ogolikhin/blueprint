﻿using System;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Models;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Reuse;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Executors
{
    [TestClass]
    public class StateChangeExecutorTests
    {
        private const int UserId = 1;
        private const int ArtifactId = 2;
        private const int WorkflowId = 3;
        private const int FromStateId = 3;
        private const int ToStateId = 4;
        private const int CurrentVersionId = 5;
        private StateChangeExecutor _stateChangeExecutor;
        private Mock<IWorkflowRepository> _workflowRepository;
        private Mock<IArtifactVersionsRepository> _artifactVersionsRepository;
        private ISqlHelper _sqlHelperMock;
        private Mock<IVersionControlService> _versionControlService;
        private Mock<IReuseRepository> _reuseRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            WorkflowStateChangeParameterEx ex = new WorkflowStateChangeParameterEx()
            {
                ArtifactId = ArtifactId,
                ToStateId = ToStateId,
                FromStateId = FromStateId,
                CurrentVersionId = CurrentVersionId
            };
            _workflowRepository = new Mock<IWorkflowRepository>(MockBehavior.Strict);
            _artifactVersionsRepository = new Mock<IArtifactVersionsRepository>(MockBehavior.Strict);
            _sqlHelperMock = new SqlHelperMock();
            _versionControlService = new Mock<IVersionControlService>(MockBehavior.Loose);
            _reuseRepository = new Mock<IReuseRepository>(MockBehavior.Loose);
            _stateChangeExecutor = new StateChangeExecutor(ex, UserId,
                _artifactVersionsRepository.Object,
                _workflowRepository.Object,
                _sqlHelperMock,
                _versionControlService.Object,
                _reuseRepository.Object
                );
        }

        [TestMethod]
        public async Task ExecuteInternal_ArtifactIsDeleted_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(true);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_ProvidedVersionIdIsIncorrect_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = 10,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_LockedByAnotherUser_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId+10
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_NoAssociatedWorkflow_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true))
                .ReturnsAsync((WorkflowState)null);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_AssociatedStateDoesNotMatchProvideCurrentState_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            var fromState = new WorkflowState
            {
                Id = FromStateId + 10,
                WorkflowId = WorkflowId,
                Name = "Ready"
            };
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_NoTransitionAvailableForStates_ThrowsConflictException()
        {
            //Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            var fromState = new WorkflowState
            {
                Id = FromStateId,
                WorkflowId = WorkflowId,
                Name = "Ready"
            };
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            _workflowRepository.Setup(
                t => t.GetTransitionForAssociatedStatesAsync(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync((WorkflowTransition)null);

            //Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            //Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_CouldNotChangeState_ReturnsFailedResult()
        {
            //Arrange
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            var fromState = new WorkflowState
            {
                Id = FromStateId,
                WorkflowId = WorkflowId,
                Name = "Ready"
            };
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            var toState = new WorkflowState
            {
                Id = ToStateId,
                Name = "Close",
                WorkflowId = WorkflowId
            };
            var transition = new WorkflowTransition()
            {
                FromState = fromState,
                Id = 10,
                ToState = toState,
                WorkflowId = WorkflowId,
                Name = "Ready to Closed"
            };
            _workflowRepository.Setup(
                t => t.GetTransitionForAssociatedStatesAsync(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync(transition);

            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(UserId, ArtifactId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync((WorkflowState)null);

            //Act
            var result = await _stateChangeExecutor.Execute();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(QueryResultCode.Failure, result.ResultCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_StateChangeAndPublish_CallsBoth()
        {
            //Arrange
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            var fromState = new WorkflowState
            {
                Id = FromStateId,
                WorkflowId = WorkflowId,
                Name = "Ready"
            };
            _workflowRepository.Setup(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true))
                .ReturnsAsync(fromState);

            var toState = new WorkflowState
            {
                Id = ToStateId,
                Name = "Close",
                WorkflowId = WorkflowId
            };
            var transition = new WorkflowTransition()
            {
                FromState = fromState,
                Id = 10,
                ToState = toState,
                WorkflowId = WorkflowId,
                Name = "Ready to Closed"
            };
            _workflowRepository.Setup(
                t => t.GetTransitionForAssociatedStatesAsync(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync(transition);

            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(UserId, ArtifactId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync((WorkflowState)null);

            //Act
            await _stateChangeExecutor.Execute();

            //Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true));
            _versionControlService.Verify(t => t.PublishArtifacts(It.IsAny<PublishParameters>(), It.IsAny<IDbTransaction>()));
        }
    }
}
