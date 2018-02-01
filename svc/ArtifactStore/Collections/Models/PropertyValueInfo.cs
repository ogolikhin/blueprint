using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class PropertyValueInfo
    {
        public int PropertyTypePredefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}