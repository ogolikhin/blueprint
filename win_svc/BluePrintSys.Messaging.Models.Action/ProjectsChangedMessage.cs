using ServiceLibrary.Models.Enums;

namespace BluePrintSys.Messaging.Models.Actions
{
    public class ProjectsChangedMessage : ActionMessage
    {
        public override MessageActionType ActionType { get; } = MessageActionType.ProjectsChanged;
    }
}
