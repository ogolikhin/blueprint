using Newtonsoft.Json;
using System;
using System.IO;

namespace FileStore.Models
{
	[JsonObject]
	public class File
	{
		[JsonProperty]
		public Guid FileId { get; set; }
		[JsonProperty]
		public DateTime StoredTime { get; set; }
		[JsonProperty]
		public DateTime? ExpiredTime { get; set; }
		[JsonProperty]
		public string FileName { get; set; }
		[JsonProperty]
		public string FileType { get; set; }
		[JsonProperty]
		public long FileSize { get; set; }
		[JsonProperty]
		public int ChunkCount { get; set; }

		public static string ConvertFileId(Guid guid)
		{
			return guid.ToString("N");
		}

		public static Guid ConvertToStoreId(string str)
		{
			try
			{
				return Guid.ParseExact(str, "N");
			}
			catch (FormatException)
			{
				return Guid.ParseExact(str, "D");
			}
		}
	}
}
