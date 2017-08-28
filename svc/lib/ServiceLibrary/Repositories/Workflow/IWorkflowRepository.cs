using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<IList<WorkflowTransition>> GetTransitionsAsync(int userId, int artifactId, int workflowId, int stateId);

        Task<WorkflowTransition> GetTransitionForAssociatedStatesAsync(int userId, int artifactId, int workflowId, int fromStateId, int toStateId);

        Task<WorkflowState> GetStateForArtifactAsync(int userId, int artifactId, int revisionId, bool addDrafts);

        Task<WorkflowState> ChangeStateForArtifactAsync(int userId, int artifactId,
            WorkflowStateChangeParameterEx stateChangeParameter, IDbTransaction transaction = null);

        Task<Dictionary<int, List<DPropertyType>>> GetCustomItemTypeToPropertiesMap(int userId, int artifactId, int projectId, IEnumerable<int> instanceItemTypeIds, IEnumerable<int> instancePropertyIds);

        Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForTransition(int userId, int artifactId, int workflowId, int fromStateId, int toStateId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="artifactId"></param>
        /// <param name="revisionId"></param>
        /// <returns></returns>
        Task<WorkflowTriggersContainer> GetWorkflowEventTriggersForNewArtifactEvent(int userId,
            int artifactId, int revisionId);
    }
}
