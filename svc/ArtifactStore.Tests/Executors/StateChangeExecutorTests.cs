﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ArtifactStore.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories.Reuse;
using ServiceLibrary.Repositories.Workflow;
using ServiceLibrary.Repositories.Webhooks;

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
        private Mock<ISaveArtifactRepository> _saveArtifactRepositoryMock;
        private Mock<IApplicationSettingsRepository> _applicationSettingsRepositoryMock;
        private Mock<IServiceLogRepository> _serviceLogRepositoryMock;
        private Mock<IUsersRepository> _usersRepositoryMock;
        private Mock<IStateChangeExecutorHelper> _stateChangeHelperMock;
        private Mock<IWorkflowEventsMessagesHelper> _workflowEventsMessagesHelperMock;
        private Mock<IWebhookRepository> _webhookRepositoryMock;

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
            _saveArtifactRepositoryMock = new Mock<ISaveArtifactRepository>(MockBehavior.Loose);
            _applicationSettingsRepositoryMock = new Mock<IApplicationSettingsRepository>(MockBehavior.Loose);
            _serviceLogRepositoryMock = new Mock<IServiceLogRepository>(MockBehavior.Loose);
            _usersRepositoryMock = new Mock<IUsersRepository>(MockBehavior.Loose);
            _stateChangeHelperMock = new Mock<IStateChangeExecutorHelper>(MockBehavior.Loose);
            _workflowEventsMessagesHelperMock = new Mock<IWorkflowEventsMessagesHelper>();
            _webhookRepositoryMock = new Mock<IWebhookRepository>(MockBehavior.Loose);
            _workflowEventsMessagesHelperMock.Setup(m => m.GenerateMessages(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<WorkflowEventTriggers>(), It.IsAny<IBaseArtifactVersionControlInfo>(), It.IsAny<string>(), It.IsAny<IDictionary<int, IList<Property>>>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IUsersRepository>(), It.IsAny<IServiceLogRepository>(), It.IsAny<IWebhookRepository>(), It.IsAny<IDbTransaction>())).ReturnsAsync(new List<IWorkflowMessage>());

            _stateChangeExecutor = new StateChangeExecutor(UserId,
                ex,
                _sqlHelperMock,
                new StateChangeExecutorRepositories(_artifactVersionsRepository.Object,
                    _workflowRepository.Object,
                    _versionControlService.Object,
                    _reuseRepository.Object,
                    _saveArtifactRepositoryMock.Object,
                    _applicationSettingsRepositoryMock.Object,
                    _serviceLogRepositoryMock.Object,
                    _usersRepositoryMock.Object,
                    _webhookRepositoryMock.Object),
                _stateChangeHelperMock.Object,
                _workflowEventsMessagesHelperMock.Object);
        }

        [TestMethod]
        public async Task ExecuteInternal_ArtifactIsDeleted_ThrowsConflictException()
        {
            // Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(true);

            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int>()))
                .ReturnsAsync(new VersionControlArtifactInfo() { Id = ArtifactId });

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_ProvidedVersionIdIsIncorrect_ThrowsConflictException()
        {
            // Arrange
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

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_LockedByAnotherUser_ThrowsConflictException()
        {
            // Arrange
            ConflictException conflictException = null;
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);

            var vcArtifactInfo = new VersionControlArtifactInfo
            {
                Id = ArtifactId,
                VersionCount = CurrentVersionId,
                LockedByUser = new UserGroup
                {
                    Id = UserId + 10
                }
            };
            _artifactVersionsRepository.Setup(t => t.GetVersionControlArtifactInfoAsync(ArtifactId, null, UserId))
                .ReturnsAsync(vcArtifactInfo);

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_NoAssociatedWorkflow_ThrowsConflictException()
        {
            // Arrange
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

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_AssociatedStateDoesNotMatchProvideCurrentState_ThrowsConflictException()
        {
            // Arrange
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

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_NoTransitionAvailableForStates_ThrowsConflictException()
        {
            // Arrange
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
            _workflowRepository.Setup(
                t => t.GetWorkflowEventTriggersForTransition(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ThrowsAsync(new ConflictException("", ErrorCodes.Conflict));

            // Act
            try
            {
                await _stateChangeExecutor.Execute();
            }
            catch (ConflictException ex)
            {
                conflictException = ex;
            }

            // Assert
            Assert.IsNotNull(conflictException);
            Assert.AreEqual(ErrorCodes.Conflict, conflictException.ErrorCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_CouldNotChangeState_ReturnsFailedResult()
        {
            // Arrange
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);
            _applicationSettingsRepositoryMock.Setup(t => t.GetTenantInfo(It.IsAny<IDbTransaction>())).ReturnsAsync(new TenantInfo()
            {
                TenantId = Guid.NewGuid().ToString()
            });

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
            _workflowRepository.Setup(
                t => t.GetWorkflowEventTriggersForTransition(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync(new WorkflowTriggersContainer());

            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(UserId, ArtifactId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync((WorkflowState)null);

            // Act
            var result = await _stateChangeExecutor.Execute();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(QueryResultCode.Failure, result.ResultCode);
        }

        [TestMethod]
        public async Task ExecuteInternal_StateChangeAndPublish_CallsBoth()
        {
            // Arrange
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);
            _applicationSettingsRepositoryMock.Setup(t => t.GetTenantInfo(It.IsAny<IDbTransaction>())).ReturnsAsync(new TenantInfo()
            {
                TenantId = Guid.NewGuid().ToString()
            });

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
            _workflowRepository.Setup(
                t => t.GetWorkflowEventTriggersForTransition(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync(new WorkflowTriggersContainer());

            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(UserId, ArtifactId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync((WorkflowState)null);

            // Act
            await _stateChangeExecutor.Execute();

            // Assert
            _workflowRepository.Verify(t => t.GetStateForArtifactAsync(UserId, ArtifactId, int.MaxValue, true));
            _versionControlService.Verify(t => t.PublishArtifacts(It.IsAny<PublishParameters>(), It.IsAny<IDbTransaction>()));
        }


        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task ExecuteInternal_WhenSynchronouTriggersErrors_ThrowsConflictException()
        {
            // Arrange
            _artifactVersionsRepository.Setup(t => t.IsItemDeleted(ArtifactId))
                .ReturnsAsync(false);
            _applicationSettingsRepositoryMock.Setup(t => t.GetTenantInfo(It.IsAny<IDbTransaction>())).ReturnsAsync(new TenantInfo()
            {
                TenantId = Guid.NewGuid().ToString()
            });

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

            var workflowEventAction = new Mock<IWorkflowEventAction>();
            workflowEventAction.Setup(a => a.ValidateAction(It.IsAny<IExecutionParameters>())).Returns(new PropertySetResult(-1, -1, ""));
            var triggerContainer = new WorkflowTriggersContainer();
            triggerContainer.SynchronousTriggers.Add(new WorkflowEventTrigger()
            {
                Action = workflowEventAction.Object,
                Name = "Test'"
            });

            _workflowRepository.Setup(
                t => t.GetWorkflowEventTriggersForTransition(UserId, ArtifactId, WorkflowId, FromStateId, ToStateId))
                .ReturnsAsync(triggerContainer);

            _workflowRepository.Setup(t => t.ChangeStateForArtifactAsync(UserId, ArtifactId, It.IsAny<WorkflowStateChangeParameterEx>(), It.IsAny<IDbTransaction>()))
                .ReturnsAsync((WorkflowState)null);

            // Act
            await _stateChangeExecutor.Execute();
        }
    }
}
