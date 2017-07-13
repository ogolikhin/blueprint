using System.Threading.Tasks;
using AdminStore.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public interface IWorkflowDataValidator
    {
        Task<WorkflowDataValidationResult> ValidateData(IeWorkflow workflow);
    }
}
