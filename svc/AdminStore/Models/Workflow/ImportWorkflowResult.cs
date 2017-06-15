using Newtonsoft.Json;

namespace AdminStore.Models.Workflow
{
    [JsonObject]
    public class ImportWorkflowResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? WorkflowId { get; set; }

        // US 7088: In XML a workflow can be specified as Active,
        // but the workflow can be created in Inactive state
        // over the conflict with other workflows. 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ErrorsGuid { get; set;  }

        [JsonIgnore]
        internal ImportWorkflowResultCodes ResultCode { get; set; }
    }

    internal enum ImportWorkflowResultCodes
    {
        Ok,
        InvalidModel,
        Conflict
    }
}