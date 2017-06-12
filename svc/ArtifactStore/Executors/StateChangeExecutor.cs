using System.Collections.Generic;
using System.Threading.Tasks;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Executors
{
    public class StateChangeExecutor : TransactionalTriggerExecutor<WorkflowStateChangeParameterEx, QuerySingleResult<WorkflowState>>
    {
        private readonly int _userId;

        private readonly IArtifactVersionsRepository _artifactVersionsRepository;
        private readonly IWorkflowRepository _workflowRepository;

        public StateChangeExecutor(
            IEnumerable<IConstraint> preOps,
            IEnumerable<IAction> postOps,
            WorkflowStateChangeParameterEx input,
            int userId,
            IArtifactVersionsRepository artifactVersionsRepository,
            IWorkflowRepository workflowRepository
            ) :
            base(preOps,
                postOps,
                input)
        {
            _userId = userId;
            _artifactVersionsRepository = artifactVersionsRepository;
            _workflowRepository = workflowRepository;
        }

        protected override async Task<QuerySingleResult<WorkflowState>> ExecuteInternal(WorkflowStateChangeParameterEx input)
        {
            //Confirm that the artifact is not deleted
            var isDeleted = await _artifactVersionsRepository.IsItemDeleted(input.ArtifactId);
            if (isDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            //Get artifact info
            var artifactInfo =
                await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(input.ArtifactId,
                    null,
                    _userId);

            if (artifactInfo.IsDeleted)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been deleted and is no longer available for workflow state change. Please refresh your view."));
            }

            if (artifactInfo.VersionCount != input.CurrentVersionId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current version of the artifact {0} does not match the specified version {1}. Please refresh your view.", artifactInfo.VersionCount, input.CurrentVersionId));
            }

            //Lock is obtained by current user inside the stored procedure itself
            //Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. Artifact is locked by another user. Please refresh your view."));
            }

            //Get current state and validate current state
            var currentState = await _workflowRepository.GetStateForArtifactAsync(_userId, input.ArtifactId, int.MaxValue, true);
            if (currentState == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. There is no workflow state associated with the artifact. Please refresh your view."));
            }
            if (currentState.Id != input.FromStateId)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("Artifact has been updated. The current workflow state id {0} of the artifact does not match the specified state {1}. Please refresh your view.", currentState.Id, input.FromStateId));
            }

            //Get available transitions and validate the required transition
            var desiredTransition =
                await _workflowRepository.GetTransitionForAssociatedStatesAsync(_userId, input.ArtifactId, currentState.WorkflowId, input.FromStateId, input.ToStateId);

            if (desiredTransition == null)
            {
                throw new ConflictException(I18NHelper.FormatInvariant("No transitions available. Workflow could have been updated. Please refresh your view."));
            }

            var newState = await _workflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input);

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

