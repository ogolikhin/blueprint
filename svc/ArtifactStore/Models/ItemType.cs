using System.Collections.Generic;
using Newtonsoft.Json;
using ServiceLibrary.Models;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class ItemType
    {
        [JsonProperty]
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ProjectId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? VersionId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? InstanceItemTypeId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Prefix { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemTypePredefined? PredefinedType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? IconImageId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? UsedInThisProject { get; set; }

        private List<int> _customPropertyTypeIds;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<int> CustomPropertyTypeIds => _customPropertyTypeIds ?? (_customPropertyTypeIds = new List<int>());
    }
}