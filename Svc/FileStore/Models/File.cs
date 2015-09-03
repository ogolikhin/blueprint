namespace FileStore.Models
{
    using Newtonsoft.Json;
    using System;

    [JsonObject]
    public class File
	{
        [JsonProperty]
        public Guid FileId { get; set; }
        [JsonProperty]
        public DateTime StoredTime { get; set; }
        [JsonProperty]
        public string FileName { get; set; }
        [JsonProperty]
        public string FileType { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public byte[] FileContent { get; set; }
	}
}
