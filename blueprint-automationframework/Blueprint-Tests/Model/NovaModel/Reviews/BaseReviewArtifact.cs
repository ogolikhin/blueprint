using Newtonsoft.Json;

namespace Model.NovaModel.Reviews
{
    public class BaseReviewArtifact
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Prefix { get; set; }

        public int ItemTypeId { get; set; }

        public int ItemTypePredefined { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public int? IconImageId { get; set; }

        public bool HasAccess { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasComments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAttachments { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasRelationships { get; set; }
    }
}
