using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowRepository
    {
        IFileRepository FileRepository { get; set; }

        Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId);

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        // Only the name and the description of DWorkflow are used.
        Task<IEnumerable<DWorkflow>> CreateWorkflowsAsync(IEnumerable<DWorkflow> workflows, int publishRevision, IDbTransaction transaction = null);
    }
}