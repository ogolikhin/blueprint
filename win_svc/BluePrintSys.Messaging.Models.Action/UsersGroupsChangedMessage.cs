using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class UsersGroupsChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.UsersGroupsChanged;

        public IEnumerable<int> UserIds { get; set; }

        public IEnumerable<int> GroupIds { get; set; }
    }
}
