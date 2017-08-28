using System.Threading.Tasks;
using AdminStore.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowDataValidator
    {
        Task<WorkflowDataValidationResult> ValidateDataAsync(IeWorkflow workflow);

        Task<WorkflowDataValidationResult> ValidateUpdateDataAsync(IeWorkflow workflow);
    }
}
