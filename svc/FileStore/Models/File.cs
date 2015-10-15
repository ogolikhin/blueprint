using Newtonsoft.Json;
using System;

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

        public static Guid ConvertToFileStoreId(string str)
        {
            return Guid.ParseExact(str, "N");
        }

        public static Guid ConvertToBlueprintStoreId(string str)
        {
            return Guid.ParseExact(str, "D");
        }
    }
}
