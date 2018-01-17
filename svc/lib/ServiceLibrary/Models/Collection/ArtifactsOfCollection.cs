using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactsOfCollection
    {
        public int ItemsCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactDto> Items { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Settings Settings { get; set; }
    }
}