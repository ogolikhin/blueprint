using ServiceLibrary.Models.Workflow;

namespace BluePrintSys.Messaging.CrossCutting.Models
{
    public abstract class GenerateAction : WorkflowEventAction, IWorkflowEventASynchronousAction
    {
    }
}
