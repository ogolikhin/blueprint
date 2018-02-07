using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models;

namespace ArtifactStore.Collections.Models
{
    public class CollectionArtifacts
    {
        public int ItemsCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactDto> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ArtifactListSettings ArtifactListSettings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Pagination Pagination { get; set; }
    }
}