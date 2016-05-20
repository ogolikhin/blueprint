using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class ApplicationLabel
    {
        [JsonProperty]
        public string Key { get; set; }
        [JsonProperty]
        public string Locale { get; set; }
        [JsonProperty]
        public string Text { get; set; }
    }
}
