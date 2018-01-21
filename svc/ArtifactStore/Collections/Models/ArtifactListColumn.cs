using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListColumn
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        public int PrimitiveType { get; set; }
    }
}