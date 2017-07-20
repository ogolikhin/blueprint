using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public GenerateTestsMessage()
        {
        }

        public GenerateTestsMessage(int tenantId) : base(tenantId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;
    }
}
