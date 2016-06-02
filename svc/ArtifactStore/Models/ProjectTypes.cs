using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class ProjectTypes
    {
        [JsonProperty]
        public List<ItemType> ArtifactTypes { get; set; }
        [JsonProperty]
        public List<ItemType> SubArtifactTypes { get; set; }
        [JsonProperty]
        public List<PropertyType> PropertyTypes { get; set; }

    }
}