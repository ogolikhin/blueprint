namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateDescendantsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateDescendants;
    }
}
