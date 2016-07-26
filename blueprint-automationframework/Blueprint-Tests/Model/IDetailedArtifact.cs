using Newtonsoft.Json;

namespace Model
{
    public interface IDetailedArtifact
    {
        [JsonProperty("id")]
        string Id { get; }

        [JsonProperty("name")]
        string Name { get; }

        [JsonProperty("description")]
        string Description { get; }

        [JsonProperty("parentId")]
        string ParentId { get; }

        [JsonProperty("orderIndex")]
        string OrderIndex { get; }

        [JsonProperty("itemTypeId")]
        string ItemTypeId { get; }

        [JsonProperty("itemTypeVersionId")]
        string ItemTypeVersionId { get; }

        [JsonProperty("lockedDateTime")]
        string LockedDateTime { get; }

        [JsonProperty("projectId")]
        string ProjectId { get; }

        [JsonProperty("version")]
        string Version { get; }

        [JsonProperty("createdOn")]
        string CreatedOn { get; }

        [JsonProperty("lastEditedOn")]
        string LastEditedOn { get; }
    }
}
