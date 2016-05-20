using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseInfo
    {
        public int LicenseLevel { get; set; }
        public int Count { get; set; }
    }
}
