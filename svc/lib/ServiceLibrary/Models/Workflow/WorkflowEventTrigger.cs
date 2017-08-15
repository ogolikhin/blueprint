using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow.Actions;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowEventTrigger
    {
        public string Name { get; set; }
        
        public WorkflowEventAction Action { get; set; }

        public WorkflowEventCondition Condition { get; set; }

        public WorkflowActionType ActionType
        {
            get
            {
                if (Action != null)
                {
                    return Action.ActionType;
                }
                return WorkflowActionType.None;
            }
        } 
    }
}
