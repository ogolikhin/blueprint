using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateTests, tenantId, workflowId)
        {
        }
    }
}
