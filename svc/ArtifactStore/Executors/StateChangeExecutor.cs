using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;

namespace ArtifactStore.Executors
{
    public sealed class StateChangeExecutor
    {
        private const string LogSource = "StateChangeExecutor";
        private readonly int _userId;
        private readonly WorkflowStateChangeParameterEx _input;
        private readonly ISqlHelper _sqlHelper;
        private readonly IStateChangeExecutorRepositories _stateChangeExecutorRepositories;
        private readonly IStateChangeExecutorHelper _stateChangeExecutorHelper;
        public StateChangeExecutor(
            int userId, WorkflowStateChangeParameterEx input,
            ISqlHelper sqlHelper,
            IStateChangeExecutorRepositories stateChangeExecutorRepositories,
            IStateChangeExecutorHelper stateChangeExecutorHelper)
        {
            _userId = userId;
            _input = input;
            _sqlHelper = sqlHelper;
            _stateChangeExecutorRepositories = stateChangeExecutorRepositories;
            _stateChangeExecutorHelper = stateChangeExecutorHelper;
        }

        public async Task<QuerySingleResult<WorkflowState>> Execute()
        {
            var result = await _sqlHelper.RunInTransactionAsync(ServiceConstants.RaptorMain, GetTransactionAction());

            return result.Result;
        }

        private Func<IDbTransaction, Task<StateChangeResult>> GetTransactionAction()
        {
            Func<IDbTransaction, Task<StateChangeResult>> action = async transaction =>
            {
                var publishRevision = await CreateRevision(transaction);
                _input.RevisionId = publishRevision;

                var artifactInfo =
                await _stateChangeExecutorRepositories.ArtifactVersionsRepository.GetVersionControlArtifactInfoAsync(_input.ArtifactId,
                    null,
                    _userId);

                await ValidateArtifact(artifactInfo);

                var currentState = await ValidateCurrentState();

                var triggers = await _stateChangeExecutorRepositories.WorkflowRepository.GetWorkflowEventTriggersForTransition(
                    _userId,
                    _input.ArtifactId,
                    currentState.WorkflowId,
                    _input.FromStateId,
                    _input.ToStateId);

                var constraints = new List<IConstraint>();


                await ProcessConstraints(constraints);

                var stateChangeResult = await ChangeStateForArtifactAsync(_input, transaction);

                var executionParameters =
                    await
                        _stateChangeExecutorHelper.BuildTriggerExecutionParameters(_userId, artifactInfo,
                            triggers.SynchronousTriggers, transaction);

                var errors = await triggers.SynchronousTriggers.ProcessTriggers(executionParameters);

                var artifactResultSet = await PublishArtifacts(publishRevision, transaction);

                // Collecting all errors so that we can distinguish between errors at a later stage.
                if (errors.Count > 0)
                {
                    await
                        _stateChangeExecutorRepositories.ServiceLogRepository.LogInformation(LogSource,
                            $"State cannot be modified as the trigger cannot be executed. {string.Join(", ", errors.Values)}");
                    throw new ConflictException("State cannot be modified as the trigger cannot be executed", ErrorCodes.WorkflowInvalidPropertyChange);
                }

                var result = new StateChangeResult
                {
                    Result = stateChangeResult
                };

            // Generate asynchronous messages for sending
            result.ActionMessages.AddRange((await WorkflowEventsMessagesHelper.GenerateMessages(
                    _userId,
                    publishRevision,
                    _input.UserName,
                    triggers.AsynchronousTriggers,
                    artifactInfo,
                    artifactResultSet?.Projects?.FirstOrDefault(d => d.Id == artifactInfo.ProjectId)?.Name,
                    artifactResultSet?.ModifiedProperties,
                    true,
                    null,
                    null,
                    _stateChangeExecutorRepositories.UsersRepository,
                    _stateChangeExecutorRepositories.ServiceLogRepository,
                    transaction)));

                await WorkflowEventsMessagesHelper.ProcessMessages(LogSource,
                    _stateChangeExecutorRepositories.ApplicationSettingsRepository,
                    _stateChangeExecutorRepositories.ServiceLogRepository,
                    result.ActionMessages,
                    $"Error on successful transition of artifact: {_input.ArtifactId} from {_input.FromStateId} to {_input.ToStateId}",
                    transaction);

                return result;
            };
            return action;
        }

        private async Task<int> CreateRevision(IDbTransaction transaction)
        {
            var publishRevision = await _sqlHelper.CreateRevisionInTransactionAsync(transaction, _userId, I18NHelper.FormatInvariant("State Change Publish: publishing changes and changing artifact {0} state to {1}", _input.ArtifactId, _input.ToStateId));
            return publishRevision;
        }

        private async Task ValidateArtifact(VersionControlArtifactInfo artifactInfo)
        {
            // Confirm that the artifact is not deleted
            var isDeleted = await _stateChangeExecutorRepositories.ArtifactVersionsRepository.IsItemDeleted(_input.ArtifactId);
            if (isDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.IsDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.VersionCount != _input.CurrentVersionId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current version of the artifact {0} does not match the specified version {1}. Please refresh your view.", artifactInfo.VersionCount, _input.CurrentVersionId));
            }

            // Lock is obtained by current user inside the stored procedure itself
            // Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. Artifact is locked by another user. Please refresh your view."));
            }
        }

        private async Task<WorkflowState> ValidateCurrentState()
        {
            // Get current state and validate current state
            var currentState = await _stateChangeExecutorRepositories.WorkflowRepository.GetStateForArtifactAsync(_userId, _input.ArtifactId, int.MaxValue, true);
            if (currentState == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. There is no workflow state associated with the artifact. Please refresh your view."));
            }
            if (currentState.Id != _input.FromStateId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current workflow state id {0} of the artifact does not match the specified state {1}. Please refresh your view.", currentState.Id, _input.FromStateId));
            }
            return currentState;
        }

        private async Task ProcessConstraints(List<IConstraint> constraints)
        {
            foreach (var constraint in constraints)
            {
                if (!(await constraint.IsFulfilled()))
                {
                    throw new ConflictException("State cannot be modified as the constrating is not fulfilled");
                }
            }
        }

        private async Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(WorkflowStateChangeParameterEx input, IDbTransaction transaction = null)
        {
            var newState = await _stateChangeExecutorRepositories.WorkflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input, transaction);

            if (newState == null)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State could not be modified for Artifact: {0} from State: {1} to New State: {2}", input.ArtifactId, input.FromStateId, input.ToStateId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = newState
            };
        }

        private async Task<ArtifactResultSet> PublishArtifacts(int publishRevision, IDbTransaction transaction)
        {
            return await _stateChangeExecutorRepositories.VersionControlService.PublishArtifacts(new PublishParameters

            {
                All = false,
                ArtifactIds = new[] { _input.ArtifactId },
                UserId = _userId,
                RevisionId = publishRevision
            }, transaction);
        }

    }
}