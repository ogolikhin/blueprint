using Newtonsoft.Json;

namespace Model.Impl
{
    public class DetailedArtifact
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("orderIndex")]
        public string OrderIndex { get; set; }

        [JsonProperty("itemTypeId")]
        public string ItemTypeId { get; set; }

        [JsonProperty("itemTypeVersionId")]
        public string ItemTypeVersionId { get; set; }

        [JsonProperty("lockedDateTime")]
        public string LockedDateTime { get; set; }

        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("createdOn")]
        public string CreatedOn { get; set; }

        [JsonProperty("lastEditedOn")]
        public string LastEditedOn { get; set; }

        [JsonProperty("createdBy")]
        public Identification CreatedBy { get; set; }

        [JsonProperty("lastEditedBy")]
        public Identification LastEditedBy { get; set; }

        [JsonProperty("lockedByUser")]
        public Identification LockedByUser { get; set; }
/*
        [JsonProperty("customProperties")]
        List<customProperty> customProperties { get; set; }*/
    }

    public class Identification
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
/*
    public class customProperty
    {
        [JsonProperty("name")]
        string Name { get; set; }

        [JsonProperty("propertyTypeId")]
        string PropertyTypeId { get; set; }

        [JsonProperty("propertyTypeVersionId")]
        string PropertyTypeVersionId { get; set; }

        [JsonProperty("propertyTypePredefined")]
        string PropertyTypePredefined { get; set; }

        [JsonProperty("value")]
        List<string> Value { get; set; }
    }*/
}
