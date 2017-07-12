using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;
using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowValidator
    {
        WorkflowValidationResult Validate(IeWorkflow workflow);
        

        
    }
}