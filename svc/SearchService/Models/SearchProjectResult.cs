using Newtonsoft.Json;

namespace SearchService.Models
{
    public class SearchProjectResult : SearchResult
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}