using Newtonsoft.Json;
using System.Collections.Generic;

namespace SearchService.Models
{
    public class SearchResult
    {
        public int ItemId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string> Path { get; set; }
    }
}
