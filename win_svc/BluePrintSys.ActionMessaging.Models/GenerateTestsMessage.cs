using NServiceBus;

namespace BluePrintSys.ActionMessaging.Models
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateTests, tenantId, workflowId)
        {
        }
    }
}
