using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class SubArtifact
    {
        [JsonProperty]
        public int ItemId { get; set; }
        [JsonProperty]
        public int ParentId { get; set; }
        [JsonProperty]
        public int ItemTypeId { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Label { get; set; }
        [JsonProperty]
        public IEnumerable<SubArtifact> Children { get; set; }
    }
}
