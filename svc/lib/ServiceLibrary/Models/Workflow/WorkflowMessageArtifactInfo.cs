using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow
{
    public class WorkflowMessageArtifactInfo : IBaseArtifactVersionControlInfo
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int ProjectId { get; set; }

        public int ItemTypeId { get; set; }

        public ItemTypePredefined PredefinedType { get; set; }

        public string ProjectName { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactPropertyInfo> ArtifactPropertyInfo { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BlueprintUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ApiLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ArtifactTypeName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string BaseArtifactType { get; set; }
    }
}
