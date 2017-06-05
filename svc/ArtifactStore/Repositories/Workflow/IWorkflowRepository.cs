using System.Threading.Tasks;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ArtifactStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<WorkflowTransitionResult> GetTransitions(int userId, int artifactId, int workflowId, int stateId);

        Task<QuerySingleResult<WorkflowState>>  GetState(int userId, int artifactId, int revisionId, bool addDrafts);

        Task<QuerySingleResult<WorkflowState>> ChangeStateForArtifact(int userId, WorkflowStateChangeParameter stateChangeParameter);

        //Task CheckForArtifactPermissions(int userId, int artifactId, int revisionId = int.MaxValue,
        //    RolePermissions permissions = RolePermissions.Read);
    }
}
