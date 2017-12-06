using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class UsersGroupsChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.UsersGroupsChanged;

    }
}
