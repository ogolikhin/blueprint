using NServiceBus;

namespace BluePrintSys.Messaging.Models.Actions
{
    [Express]
    public class StateChangeMessage : ActionMessage
    {
        public StateChangeMessage()
        {
        }

        public StateChangeMessage(int tenantId) : base(tenantId)
        {
        }

        public override MessageActionType ActionType { get; } = MessageActionType.StateChange;

        public int ArtifactId { get; set; }
        public int RevisionId { get; set; }
        public string ArtifactType { get; set; }
        public int CurrentStateId { get; set; }
        public int PreviousStateId { get; set; }
    }
}
