using Newtonsoft.Json;

namespace AdminStore.Models.Workflow
{
    [JsonObject]
    public class ImportWorkflowResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? WorkflowId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorsGuid { get; set;  }
    }
}