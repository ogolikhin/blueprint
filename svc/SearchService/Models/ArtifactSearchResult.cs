using Newtonsoft.Json;
using ServiceLibrary.Models;

namespace SearchService.Models
{
    [JsonObject]
    public class ArtifactSearchResult : SearchResult
    {
        [JsonProperty]
        public int Id
        {
            get { return ItemId; }
            set { ItemId = value; }
        }

        [JsonProperty]
        public int ProjectId { get; set; }

        [JsonProperty]
        public string ProjectName { get; set; }

        [JsonProperty]
        public int ItemTypeId { get; set; }

        [JsonProperty]
        public string TypePrefix { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ItemTypeIconId { get; set; }

        [JsonProperty]
        public ItemTypePredefined PredefinedType { get; set; }

        [JsonProperty]
        public bool HasReadPermission { get; set; }
    }
}