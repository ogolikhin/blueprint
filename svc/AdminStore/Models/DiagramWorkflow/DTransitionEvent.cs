using System.Collections.Generic;
using AdminStore.Models.Enums;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DTransitionEvent : DEvent
    {
        public override EventTypes EventType => EventTypes.Transition;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FromState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? FromStateId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ToState { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ToStateId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DGroup> PermissionGroups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SkipPermissionGroups { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DPortPair PortPair { get; set; }
    }
}