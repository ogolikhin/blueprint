using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class Artifact : Item, IArtifact
    {
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedOn { get; set; }
    }

    [JsonObject]
    public class Property
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PropertyTypeId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PropertyTypePredefined Predefined { get; set; }
    }
}
