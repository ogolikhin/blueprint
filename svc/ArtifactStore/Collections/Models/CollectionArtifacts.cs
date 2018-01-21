using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class CollectionArtifacts
    {
        public int ItemsCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactDto> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ArtifactListSettings ArtifactListSettings { get; set; }
    }
}