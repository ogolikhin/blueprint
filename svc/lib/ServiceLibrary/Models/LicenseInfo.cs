using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseInfo
    {
        [JsonProperty]
        public int LicenseLevel { get; set; }
		[JsonProperty]
		public int Count { get; set; }
	}
}
