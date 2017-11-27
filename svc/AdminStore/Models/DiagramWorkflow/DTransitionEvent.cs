using System.Collections.Generic;
using AdminStore.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DTransitionEvent : DEvent
    {
        public override EventTypes EventType => EventTypes.Transition;
        public string FromState { get; set; }
        public int? FromStateId { get; set; }
        public string ToState { get; set; }
        public int? ToStateId { get; set; }
        public IEnumerable<DGroup> PermissionGroups { get; set; }
        public bool? SkipPermissionGroups { get; set; }
        public DPortPair PortPair { get; set; }
    }
}