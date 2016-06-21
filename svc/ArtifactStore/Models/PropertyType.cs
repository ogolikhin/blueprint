using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class PropertyType
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? VersionId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? InstancePropertyTypeId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRichText { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? DecimalDefaultValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateDefaultValue { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<UserGroup> UserGroupDefaultValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StringDefaultValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DecimalPlaces { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MaxNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MinNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? MaxDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? MinDate { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMultipleAllowed { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRequired { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValidated { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        public List<string> ValidValues { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DefaultValidValueIndex { get; set; }
    }
}