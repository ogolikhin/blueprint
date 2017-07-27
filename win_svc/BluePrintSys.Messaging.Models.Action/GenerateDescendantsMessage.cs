using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class GenerateDescendantsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateDescendants;
    }
}
