using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        Task<ImportWorkflowResult> ImportWorflowAsync(IeWorkflow workflow, int userId);

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        // Only the name and the description of DWorkflow are used.
        Task<IEnumerable<DWorkflow>> CreateWorkflowsAsync(IEnumerable<DWorkflow> workflows, int publishRevision, IDbTransaction transaction = null);
    }
}