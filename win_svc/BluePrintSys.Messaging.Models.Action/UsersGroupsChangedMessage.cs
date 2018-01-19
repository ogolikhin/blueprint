using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public enum UsersGroupsChangedType
    {
        Create,
        Update,
        Delete
    }

    public class UsersGroupsChangedMessage : ActionMessage
    {
        public UsersGroupsChangedMessage()
        {
            UserIds = new List<int>();
            GroupIds = new List<int>();
        }

        public UsersGroupsChangedMessage(IEnumerable<int> userIds, IEnumerable<int> groupIds)
        {
            // using Lists to allow deserialization
            UserIds = userIds.ToList();
            GroupIds = groupIds.ToList();
        }

        public override MessageActionType ActionType { get; } = MessageActionType.UsersGroupsChanged;

        public IEnumerable<int> UserIds { get; set; }

        public IEnumerable<int> GroupIds { get; set; }

        public UsersGroupsChangedType ChangeType { get; set; }
    }
}
