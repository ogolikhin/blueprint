using System.Collections.Generic;
using Newtonsoft.Json;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactListSettings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactListFilter> Filters { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactListColumn> Columns { get; set; }
    }
}