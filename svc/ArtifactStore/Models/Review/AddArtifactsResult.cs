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
        internal IEnumerable<int> ArtifactIds { get; set; }
        internal int NotExisted { get; set; }
        internal int Unpublished { get; set; }
        internal int ProjectMoved { get; set; }
    }
}
