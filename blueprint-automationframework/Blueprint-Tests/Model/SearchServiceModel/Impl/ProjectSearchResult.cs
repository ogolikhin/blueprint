using Newtonsoft.Json;

namespace Model.SearchServiceModel.Impl
{
    public class ProjectSearchResult
    {
        [JsonProperty("id")]
        public int ProjectId { get; set; }

        [JsonProperty("name")]
        public string ProjectName { get; set; }

        public string Path { get; set; }
    }
}
