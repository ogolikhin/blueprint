using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<IList<WorkflowTransition>> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<WorkflowTransition> GetTransitionForAssociatedStatesAsync(int userId, int artifactId, int workflowId, int fromStateId, int toStateId);

        Task<WorkflowState> GetStateForArtifactAsync(int userId, int artifactId, int revisionId, bool addDrafts);

        Task<WorkflowState> ChangeStateForArtifactAsync(int userId, int artifactId,
            WorkflowStateChangeParameter stateChangeParameter);
    }
}
