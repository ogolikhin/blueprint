using Newtonsoft.Json;

namespace AdminStore.Models.DTO
{
    [JsonObject]
    public class ProjectFolderSearchDto
    {
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Location { get; set; }
        public InstanceItemTypeEnum Type { get; set; }
    }
}