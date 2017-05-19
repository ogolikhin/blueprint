using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories.Workflow
{
    public interface ISqlWorkflowRepository
    {
        Task<WorkflowTransitionResult> GetTransitions(int artifactId, int userId);

        Task<WorkflowState>  GetCurrentState(int userId, int itemId, int revisionId = int.MaxValue, bool addDrafts = true);

        Task<WorkflowTransitionResult> GetAvailableTransitions(int userId, int workflowId, int stateId);
    }
}
