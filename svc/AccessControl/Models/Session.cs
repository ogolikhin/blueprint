using Newtonsoft.Json;
using System;

namespace AccessControl.Models
{
    [JsonObject]
    public class Session
    {
        [JsonProperty]
        public int UserId { get; set; }
        //[JsonProperty] - do not send session id
        public Guid SessionId { get; set; }
        [JsonProperty]
        public DateTime? BeginTime { get; set; }
        [JsonProperty]
        public DateTime? EndTime { get; set; }
		[JsonProperty]
	    public string UserName { get; set; }
		[JsonProperty]
	    public int LicenseLevel { get; set; }

	    public static string Convert(Guid guid)
        {
            return guid.ToString(("N"));
        }

        public static Guid Convert(string val)
        {
            return Guid.ParseExact(val, "N");
        }
    }
}
