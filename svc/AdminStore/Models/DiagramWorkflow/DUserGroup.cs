using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DUserGroup
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsGroup { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GroupProjectPath { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? GroupProjectId { get; set; }
    }
}