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

        Task<IEnumerable<SqlTrigger>> CreateWorkflowTriggersAsync(IEnumerable<SqlTrigger> workflowTriggers,
            int publishRevision, IDbTransaction transaction = null);

        Task CreateWorkflowArtifactAssociationsAsync(IEnumerable<string> artifactTypeNames,
            IEnumerable<int> projectIds, int workflowId, int publishRevision, IDbTransaction transaction = null);

        Task<IEnumerable<SqlProjectPathPair>> GetProjectIdsByProjectPaths(IEnumerable<string> projectPaths);

        Task<int> CreateRevisionInTransactionAsync(IDbTransaction transaction, int userId, string description);

        Task<IEnumerable<string>> CheckLiveWorkflowsForNameUniqueness(IDbTransaction transaction,
            IEnumerable<string> names);

        Task RunInTransactionAsync(Func<IDbTransaction, Task> action);

        Task<QueryResult<WorkflowDto>> GetWorkflows(Pagination pagination, Sorting sorting = null, string search = null,
            Func<Sorting, string> sort = null);
    }
}