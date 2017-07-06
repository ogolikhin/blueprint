using NServiceBus;

namespace BluePrintSys.Messaging.Models.Action
{
    [Express]
    public class GenerateDescendantsMessage : ActionMessage
    {
        public GenerateDescendantsMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateDescendants, tenantId, workflowId)
        {
        }
    }
}
