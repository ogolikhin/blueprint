using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class ArtifactResultSet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Artifact> Artifacts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Item> Projects { get; set; }
    }

}
