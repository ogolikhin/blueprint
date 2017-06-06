using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtifactStore.Repositories;
using ArtifactStore.Repositories.Workflow;
using ServiceLibrary.Exceptions;
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
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Get artifact info
            var artifactInfo =
                await _artifactVersionsRepository.GetVersionControlArtifactInfoAsync(input.ArtifactId,
                    null,
                    _userId);

            if (artifactInfo.VersionCount > input.CurrentVersionId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Lock is obtained by current user inside the stored procedure itself
            //Check that it is not locked by some other user
            if (artifactInfo.LockedByUser != null && artifactInfo.LockedByUser.Id != _userId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Get current state and validate current state
            var currentState = await _workflowRepository.GetStateForArtifactAsync(_userId, input.ArtifactId, int.MaxValue, true);
            if (currentState == null ||
                currentState.ResultCode != QueryResultCode.Success ||
                currentState.Item == null ||
                currentState.Item.Id != input.FromStateId)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            //Get available transitions and validate the required transition
            var availableTransitions =
                await _workflowRepository.GetTransitionsAsync(_userId, input.ArtifactId, currentState.Item.WorkflowId, input.FromStateId);
            if (availableTransitions.Total == 0 ||
                availableTransitions.ResultCode != QueryResultCode.Success)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            var desiredTransition =
                availableTransitions.Items.FirstOrDefault(tr => tr.FromState.Id == input.FromStateId &&
                                                               tr.ToState.Id == input.ToStateId);

            if (desiredTransition == null)
            {
                throw new ConflictException("Artifact has been updated. Please refresh your view.");
            }

            var stateChangeResult = await  _workflowRepository.ChangeStateForArtifactAsync(_userId, input.ArtifactId, input);

            return stateChangeResult;
        }
    }
}

