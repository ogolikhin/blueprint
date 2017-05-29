using Newtonsoft.Json;

namespace ArtifactStore.Models.Review
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
