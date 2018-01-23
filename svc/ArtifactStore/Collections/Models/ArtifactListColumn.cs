using Newtonsoft.Json;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListColumn
    {
        public string PropertyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PrimitiveType { get; set; }
    }
}
