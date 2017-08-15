using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId);

        Task<ImportWorkflowResult> UpdateWorkflowViaImport(int workflowId, IeWorkflow workflow, string fileName, int userId);

        IFileRepository FileRepository { get; set; }

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        Task<WorkflowDto> GetWorkflowDetailsAsync(int workflowId);

        Task UpdateWorkflowStatusAsync(StatusUpdate statusUpdate, int workflowId, int userId);

        Task<int> DeleteWorkflows(OperationScope body, string search, int sessionUserId);

        Task<IeWorkflow> GetWorkflowExportAsync(int workflowId);
    }
}
