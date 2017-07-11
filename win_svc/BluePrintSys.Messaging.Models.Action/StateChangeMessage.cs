using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class StateChangeMessage : ActionMessage
    {
        public StateChangeMessage()
        {
            
        }

        public StateChangeMessage(int tenantId, int workflowId) : base(tenantId, workflowId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.StateChange;
    }
}
