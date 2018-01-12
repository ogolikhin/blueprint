using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public enum PropertyItemTypeChangeType
    {
        None = 0,
        PropertyType = 1,
        ItemType = 2
    }
    public class PropertyItemTypesChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.PropertyItemTypesChanged;

        public IEnumerable<int> PropertyTypeIds { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }

        public bool IsStandard { get; set; }

        public PropertyItemTypeChangeType ChangeType { get; set; }
    }
}
