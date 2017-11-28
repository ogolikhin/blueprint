using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow {
    public class DTrigger
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        public DBaseAction Action { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DStateCondition Condition { get; set; }
    }
}