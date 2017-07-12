using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Models.Workflow;

namespace AdminStore.Repositories.Workflow
{
    public interface IWorkflowDataValidator
    {
        Task<WorkflowDataValidationResult> ValidateData(IeWorkflow workflow);
    }
}
