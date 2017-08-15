using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class TenantInfo
    {
        [JsonProperty]
        public string TenantId { get; set; }

        [JsonProperty]
        public string TenantName { get; set; }
    }
}