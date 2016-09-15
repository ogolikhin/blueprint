﻿using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class SubArtifact
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty]
        public int ParentId { get; set; }
        [JsonProperty]
        public int ItemTypeId { get; set; }
        [JsonProperty]
        public string DisplayName { get; set; }
        [JsonProperty]
        public ItemTypePredefined PredefinedType { get; set; }
        [JsonProperty]
        public bool HasChildren { get; set; } = false;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<SubArtifact> Children { get; set; }
    }
}
