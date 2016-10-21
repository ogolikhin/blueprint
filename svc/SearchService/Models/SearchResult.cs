using Newtonsoft.Json;

namespace SearchService.Models
{
    public class SearchResult
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }
    }
}
