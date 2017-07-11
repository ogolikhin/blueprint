using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateDescendantsMessage : ActionMessage
    {
        public GenerateDescendantsMessage()
        {
            
        }

        public GenerateDescendantsMessage(int tenantId, int workflowId) : base(tenantId, workflowId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateDescendants;
    }
}
