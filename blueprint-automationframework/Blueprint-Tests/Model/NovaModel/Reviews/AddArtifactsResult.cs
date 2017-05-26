using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    public class AddArtifactsResult
    {
        public int ArtifactCount { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int AlreadyIncludedArtifactCount { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int UnpublishedArtifactCount { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NonexistentArtifactCount { get; set; }
    }
}