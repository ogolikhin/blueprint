using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<WorkflowTransitionResult> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>> GetStateForArtifactAsync(int userId, int artifactId, int revisionId, bool addDrafts);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifactAsync(int userId, int artifactId,
            WorkflowStateChangeParameter stateChangeParameter);
    }
}
