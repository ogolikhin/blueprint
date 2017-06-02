using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class ApplicationSetting
    {
        [JsonProperty]
        public string Key { get; set; }

        [JsonProperty]
        public string Value { get; set; }

        [JsonProperty]
        public bool Restricted { get; set; }

    }
}