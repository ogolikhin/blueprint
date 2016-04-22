using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class ResetPostContent
    {
        [JsonProperty]
        public string NewPass { get; set; }
        [JsonProperty]
        public string OldPass { get; set; }
    }
}