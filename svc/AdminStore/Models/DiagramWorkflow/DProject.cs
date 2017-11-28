using System.Collections.Generic;
using Newtonsoft.Json;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DProject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<DArtifactType> ArtifactTypes { get; set; }
    }
}