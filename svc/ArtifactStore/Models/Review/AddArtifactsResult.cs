using Newtonsoft.Json;
using System.Collections.Generic;

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

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ProjectMovedCount { get; set; }
    }

    internal class EffectiveArtifactIdsResult
    {
        public IEnumerable<int> ArtifactIds { get; set; }
        public int Nonexistent { get; set; }
        public int Unpublished { get; set; }
        public int ProjectMoved { get; set; }
    }
}
