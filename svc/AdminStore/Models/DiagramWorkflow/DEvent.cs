using System.Collections.Generic;
using AdminStore.Models.Enums;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public abstract class DEvent
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        public abstract EventTypes EventType { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DTrigger> Triggers { get; set; }
    }
}