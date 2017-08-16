namespace BluePrintSys.Messaging.Models.Actions
{
    public class GenerateTestsMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.GenerateTests;
    }
}
