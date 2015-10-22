using Newtonsoft.Json;
using System;

namespace AdminStore.Models
{
	[JsonObject]
	public class User
	{
		[JsonProperty]
		public int UserId { get; set; }
		[JsonProperty]
		public Guid SessionId { get; set; }
		[JsonProperty]
		public DateTime? BeginTime { get; set; }
		[JsonProperty]
		public DateTime? EndTime { get; set; }

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
