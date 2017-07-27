using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateTestsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;
    }
}
