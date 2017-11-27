using System.Collections.Generic;
using AdminStore.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public abstract class DEvent
    {
        public int? Id { get; set; }
        public abstract EventTypes EventType { get; }
        public string Name { get; set; }
        public IEnumerable<DTrigger> Triggers { get; set; }
    }
}