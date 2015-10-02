using Newtonsoft.Json;
using System;

namespace AccessControl.Models
{
	[JsonObject]
	public class Session
	{
		[JsonProperty]
		public Guid SessionId { get; set; }
		[JsonProperty]
		public int UserId { get; set; }
		[JsonProperty]
		public DateTime BeginTime { get; set; }
		[JsonProperty]
		public DateTime EndTime { get; set; }

		public string SessionKey
		{
			get { return SessionId.ToString(("N")); }
			set { Guid.ParseExact(value, "N"); }
		}
	}
}
