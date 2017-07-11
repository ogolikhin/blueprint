using System.Threading.Tasks;
using AdminStore.Models.Workflow;
using ServiceLibrary.Repositories.Files;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowService
    {
        Task<ImportWorkflowResult> ImportWorkflowAsync(IeWorkflow workflow, string fileName, int userId);

        IFileRepository FileRepository { get; set; }

        Task<string> GetImportWorkflowErrorsAsync(string guid, int userId);

        Task<WorkflowDto> GetWorkflowDetailsAsync(int workflowId);

        Task UpdateWorkflowStatusAsync(WorkflowDto workflowDto, int workflowId, int userId);
    }
}
