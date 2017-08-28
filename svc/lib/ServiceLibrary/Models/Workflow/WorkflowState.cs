using Newtonsoft.Json;

namespace ServiceLibrary.Models.Workflow
{
    [JsonObject]
    public class WorkflowState
    {
        public int WorkflowId { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDefault { get; set; }
    }
}