using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class Artifact : IArtifact
    {
        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty]
        public int ProjectId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ParentId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prefix { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemTypePredefined? PredefinedType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Version { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public double? OrderIndex { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasChildren { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RolePermissions? Permissions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UserGroup LockedByUser { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? LockedDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<IArtifact> Children { get; set; }
    }
}
