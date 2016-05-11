using System;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    public class ArtifactInfo : IArtifactInfo
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("projectId")]
        public int ProjectId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("typePrefix")]
        public string TypePrefix { get; set; }
        [JsonProperty("baseItemTypePredefined")]
        public int BaseTypePredefined { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
