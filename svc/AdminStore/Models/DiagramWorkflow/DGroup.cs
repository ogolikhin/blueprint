using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DGroup
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }
}