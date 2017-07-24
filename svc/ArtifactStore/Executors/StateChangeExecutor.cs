using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ArtifactStore.Services.VersionControl;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Executors
{
    public sealed class StateChangeExecutor : TransactionalTriggerExecutor<WorkflowStateChangeParameterEx, QuerySingleResult<WorkflowState>>
    {
        private readonly int _userId;
        
        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IVersionControlService _versionControlService;

        private readonly IList<IConstraint> _constraints = new List<IConstraint>();
        private readonly IList<WorkflowEventTrigger> _preOpTriggers = new List<WorkflowEventTrigger>();
        private readonly IList<WorkflowEventTrigger> _postOpTriggers = new List<WorkflowEventTrigger>();

        public StateChangeExecutor(
            WorkflowStateChangeParameterEx input,
            int userId,
            IArtifactVersionsRepository artifactVersionsRepository,
            IWorkflowRepository workflowRepository,
            ISqlHelper sqlHelper,
            IVersionControlService versionControlService
            ) :
            base(sqlHelper,
                input)
        {
            _userId = userId;
            _artifactVersionsRepository = artifactVersionsRepository;
            _workflowRepository = workflowRepository;
            _versionControlService = versionControlService;
        }

        protected override Func<IDbTransaction, Task<QuerySingleResult<WorkflowState>>> GetTransactionAction()
        {
            Func<IDbTransaction, Task<QuerySingleResult<WorkflowState>>> action = async transaction =>
            {
                var publishRevision =
                    await
                        SqlHelper.CreateRevisionInTransactionAsync(
                            transaction,
                            _userId,
                            I18NHelper.FormatInvariant(
                                "State Change Publish: publishing changes and changing artifact {0} state to {1}",
                                Input.ArtifactId, Input.ToStateId));
                Input.RevisionId = publishRevision;



                //Confirm that the artifact is not deleted
                var isDeleted = await _artifactVersionsRepository.IsItemDeleted(Input.ArtifactId);
                if (isDeleted)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
                }

                //Get artifact info
                var artifactInfo =
                    await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(Input.ArtifactId,
                        null,
                        _userId);

                if (artifactInfo.IsDeleted)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
                }

                if (artifactInfo.VersionCount != Input.CurrentVersionId)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been updated. The current version of the artifact {0} does not match the specified version {1}. Please refresh your view.",
                            artifactInfo.VersionCount, Input.CurrentVersionId));
                }

                //Lock is obtained by current user inside the stored procedure itself
                //Check that it is not locked by some other user
                if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been updated. Artifact is locked by another user. Please refresh your view."));
                }

                //Get current state and validate current state
                var currentState =
                    await _workflowRepository.GetStateForArtifactAsync(_userId, Input.ArtifactId, int.MaxValue, true);
                if (currentState == null)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been updated. There is no workflow state associated with the artifact. Please refresh your view."));
                }
                if (currentState.Id != Input.FromStateId)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "Artifact has been updated. The current workflow state id {0} of the artifact does not match the specified state {1}. Please refresh your view.",
                            currentState.Id, Input.FromStateId));
                }

                //Get available transitions and validate the required transition
                var desiredTransition =
                    await
                        _workflowRepository.GetTransitionForAssociatedStatesAsync(_userId, Input.ArtifactId,
                            currentState.WorkflowId, Input.FromStateId, Input.ToStateId);

                if (desiredTransition == null)
                {
                    throw new ConflictException(
                        I18NHelper.FormatInvariant(
                            "No transitions available. Workflow could have been updated. Please refresh your view."));
                }

                foreach (var workflowEventTrigger in desiredTransition.Triggers.Where(t => t?.Action != null))
                {
                    if (workflowEventTrigger.Action is ISynchronousAction)
                    {
                        _preOpTriggers.Add(workflowEventTrigger);
                    }
                    else if (workflowEventTrigger.Action is IASynchronousAction)
                    {
                        _postOpTriggers.Add(workflowEventTrigger);
                    }
                }

                foreach (var constraint in _constraints)
                {
                    if (!(await constraint.IsFulfilled()))
                    {
                        throw new ConflictException("State cannot be modified as the constrating is not fulfilled");
                    }
                }

                var result = await ExecuteInternal(Input, transaction);


                foreach (var triggerExecutor in _preOpTriggers)
                {
                    if (!await triggerExecutor.Action.Execute())
                    {
                        throw new ConflictException("State cannot be modified as the trigger cannot be executed");
                    }
                }

                await _versionControlService.PublishArtifacts(new PublishParameters

                {
                    All = false,
                    ArtifactIds = new[] {Input.ArtifactId},
                    UserId = _userId,
                    RevisionId = publishRevision
                }, transaction);

                foreach (var triggerExecutor in _postOpTriggers)
                {
                    if (!await triggerExecutor.Action.Execute())
                    {
                        throw new ConflictException("State cannot be modified as the trigger cannot be executed");
                    }
                }
                return result;
            };
            return action;
        }

        protected override async Task<QuerySingleResult<WorkflowState>> ExecuteInternal(WorkflowStateChangeParameterEx input, IDbTransaction transaction = null)
        {
            var newState = await _workflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input, transaction);

            if (newState == null)
            {
                return new QuerySingleResult<WorkflowState>
                {
                    ResultCode = QueryResultCode.Failure,
                    Message = I18NHelper.FormatInvariant("State could not be modified for Artifact: {0} from State: {1} to New State: {2}", 
                    input.ArtifactId, 
                    input.FromStateId, 
                    input.ToStateId)
                };
            }
            return new QuerySingleResult<WorkflowState>
            {
                ResultCode = QueryResultCode.Success,
                Item = newState
            };
        }
    }
}

