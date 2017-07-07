using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class Item
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
        public int? ItemTypeIconId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ItemTypePredefined? PredefinedType { get; set; }
    }
}
