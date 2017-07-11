using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage()
        {
            
        }

        public GenerateTestsMessage(int tenantId, int workflowId) : base(tenantId, workflowId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;
    }
}
