using NServiceBus;

namespace BluePrintSys.Messaging.Models.Action
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage(int tenantId, int workflowId) : base(MessageActionType.GenerateTests, tenantId, workflowId)
        {
        }
    }
}
