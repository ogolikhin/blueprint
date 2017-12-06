using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class PropertyItemTypeChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.PropertyItemTypesChanged;
    }
}
