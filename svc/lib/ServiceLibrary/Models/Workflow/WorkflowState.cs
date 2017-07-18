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
        public string Description { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDefault { get; set; }
    }

    public class SqlWorkFlowState
    {
        public int? Result { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowStateName { get; set; }
        public int WorkflowStateId { get; set; }
    }

    public class SqlWorkFlowStateInformation : SqlWorkFlowState
    {
        public string WorkflowName { get; set; }
        public int ProjectId { get; set; }
        public int ArtifactId { get; set; }
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int ItemTypeId { get; set; }
        public int StartRevision { get; set; }
        public int EndRevision { get; set; }
        public int? LockedByUserId { get; set; }
    }
}