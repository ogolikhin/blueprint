using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class PropertyInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public string Value { get; set; }
    }
}