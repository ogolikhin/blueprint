using Newtonsoft.Json;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DGenerateAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.Generate;

        public GenerateActionTypes GenerateActionType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ChildCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArtifactType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ArtifactTypeId { get; set; }
    }
}