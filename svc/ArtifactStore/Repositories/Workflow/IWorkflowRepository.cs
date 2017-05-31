using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<WorkflowTransitionResult> GetTransitions(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>>  GetCurrentState(int userId, int artifactId, int revisionId = int.MaxValue, bool addDrafts = true);
    }
}
