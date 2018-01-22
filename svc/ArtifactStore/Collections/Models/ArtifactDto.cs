using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactDto
    {
        public int ArtifactId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PredefinedType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeIconId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<PropertyValueInfo> PropertyInfos { get; set; }
    }
}