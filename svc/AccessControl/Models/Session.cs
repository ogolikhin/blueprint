using Newtonsoft.Json;
using System;

namespace AccessControl.Models
{
	[JsonObject]
	public class Session
	{
		[JsonProperty]
		public Guid FileId { get; set; }
		[JsonProperty]
		public DateTime StoredTime { get; set; }
		[JsonProperty]
		public string FileName { get; set; }
		[JsonProperty]
		public string FileType { get; set; }
		[JsonProperty]
		public long FileSize { get; set; }
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public byte[] FileContent { get; set; }

		public static string ConvertFileId(Guid guid)
		{
			return guid.ToString("N");
		}

		public static Guid ConvertFileId(string str)
		{
			return Guid.ParseExact(str, "N");
		}
	}
}
