using AdminStore.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DPropertyChangeEvent : DEvent
    {
        public override EventTypes EventType => EventTypes.PropertyChange;
        public string PropertyName { get; set; }
        public int? PropertyId { get; set; }
    }
}