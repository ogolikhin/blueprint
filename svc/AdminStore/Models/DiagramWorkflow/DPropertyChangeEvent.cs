using AdminStore.Models.Enums;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DPropertyChangeEvent : DEvent
    {
        public override EventTypes EventType => EventTypes.PropertyChange;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyId { get; set; }
    }
}