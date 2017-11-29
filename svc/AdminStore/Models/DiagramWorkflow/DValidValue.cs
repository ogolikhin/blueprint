using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DValidValue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}