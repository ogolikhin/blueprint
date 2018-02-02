using System.Collections.Generic;
using ArtifactStore.ArtifactList.Models;
using Newtonsoft.Json;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListSettings
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ArtifactListFilter> Filters { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ProfileColumn> Columns { get; set; }
    }
}