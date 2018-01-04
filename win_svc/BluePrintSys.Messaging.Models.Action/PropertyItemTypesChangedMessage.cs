using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class PropertyItemTypesChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.PropertyItemTypesChanged;

        public IEnumerable<int> PropertyTypeIds { get; set; }

        public IEnumerable<int> ItemTypeIds { get; set; }

        public bool IsStandard { get; set; }
    }
}
