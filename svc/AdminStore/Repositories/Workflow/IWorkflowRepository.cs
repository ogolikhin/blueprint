using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        IFileRepository FileRepository { get; set; }


        // Only the name and the description of DWorkflow are used.
        Task<IEnumerable<SqlWorkflow>> CreateWorkflowsAsync(IEnumerable<SqlWorkflow> workflows, int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlState>> CreateWorkflowStatesAsync(IEnumerable<SqlState> workflowStates, int publishRevision,
            IDbTransaction transaction = null);

        Task<IEnumerable<SqlWorkflowEvent>> CreateWorkflowEventsAsync(IEnumerable<SqlWorkflowEvent> workflowEvents,
            int publishRevision, IDbTransaction transaction = null);

        Task CreateWorkflowArtifactAssociationsAsync(IEnumerable<string> artifactTypeNames,
            IEnumerable<int> projectIds, int workflowId, int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlProjectPathPair>> GetProjectIdsByProjectPaths(IEnumerable<string> projectPaths);

        Task<IEnumerable<SqlArtifactTypesWorkflowDetails>> GetExistingStandardArtifactTypesForWorkflows(IEnumerable<string> artifactTypes, IEnumerable<int> projectIds);

        Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string description);

        Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniqueness(IDbTransaction transaction,
            IEnumerable<string> names);

        Task RunInTransactionAsync(Func<IDbTransaction, Task> action);

        Task<QueryResult<WorkflowDto>> GetWorkflows(Pagination pagination, Sorting sorting = null, string search = null,
            Func<Sorting, string> sort = null);

        Task<SqlWorkflow> GetWorkflowDetailsAsync(int workflowId);

        Task<IEnumerable<SqlWorkflowArtifactTypesAndProjects>> GetWorkflowArtifactTypesAndProjectsAsync(int workflowId);

        Task<int> DeleteWorkflows(OperationScope body, string search, int revision);

        Task<IEnumerable<SqlWorkflow>> UpdateWorkflows(IEnumerable<SqlWorkflow> workflows, int revision,
            IDbTransaction transaction = null);
    }
}