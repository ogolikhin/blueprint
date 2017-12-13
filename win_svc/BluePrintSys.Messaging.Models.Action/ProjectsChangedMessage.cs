using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class ProjectsChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.ProjectsChanged;

        public IEnumerable<int> ProjectIds { get; set; }
    }
}
