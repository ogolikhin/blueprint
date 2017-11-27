using System.Threading.Tasks;
using AdminStore.Models.Enums;
using AdminStore.Models.Workflow;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId, string xmlSerError);

        Task<ImportWorkflowResult> UpdateWorkflowViaImport(int userId, int workflowId, IeWorkflow workflow, string fileName = null, string xmlSerError = null, WorkflowMode workflowMode = WorkflowMode.XmlExport);

        IFileRepository FileRepository { get; set; }

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        Task<WorkflowDetailsDto> GetWorkflowDetailsAsync(int workflowId);

        Task<int> UpdateWorkflowStatusAsync(StatusUpdate statusUpdate, int workflowId, int userId);

        Task UpdateWorkflowAsync(UpdateWorkflowDto workflowDto, int workflowId, int userId);

        Task<int> DeleteWorkflows(OperationScope body, string search, int sessionUserId);

        Task<IeWorkflow> GetWorkflowExportAsync(int workflowId, WorkflowMode mode);
        Task<int> CreateWorkflow(string name, string description, int userId);
    }
}
