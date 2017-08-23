using BluePrintSys.Messaging.CrossCutting.Models;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Models.Workflow
{
    public class WorkflowEventTrigger
    {
        public string Name { get; set; }
        
        public WorkflowEventAction Action { get; set; }

        public WorkflowEventCondition Condition { get; set; }

        public MessageActionType ActionType
        {
            get
            {
                if (Action != null)
                {
                    return Action.ActionType;
                }
                return MessageActionType.None;
            }
        } 
    }
}
