using AdminStore.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DNewArtifactEvent : DEvent
    {
        public override EventTypes EventType => EventTypes.NewArtifact;
    }
}