using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactDto
    {
        public int ArtifactId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<PropertyInfo> PropertyInfos { get; set; }
    }
}