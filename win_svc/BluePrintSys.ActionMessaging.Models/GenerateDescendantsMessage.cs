using NServiceBus;

namespace BluePrintSys.ActionMessaging.Models
{
    [Express]
    public class GenerateDescendantsMessage : ActionMessage
    {
        public GenerateDescendantsMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateDescendants, tenantId, workflowId)
        {
        }
    }
}
