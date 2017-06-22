using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class ArtifactResultSet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Artifact> Artifacts { get; } = new List<Artifact>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Item> Projects { get;  } = new List<Item>();
    }

}
