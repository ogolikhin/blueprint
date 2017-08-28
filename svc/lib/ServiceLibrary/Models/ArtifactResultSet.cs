using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class ArtifactResultSet
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? RevisionId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Artifact> Artifacts { get; } = new List<Artifact>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<Item> Projects { get;  } = new List<Item>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonIgnore]
        public IDictionary<int, IList<Property>> ModifiedProperties { get; } = new Dictionary<int, IList<Property>>();
    }

}
