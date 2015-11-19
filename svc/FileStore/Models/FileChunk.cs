using Newtonsoft.Json;
using System;
using System.IO;

namespace FileStore.Models
{
    [JsonObject]
    public class FileChunk
    {
        [JsonProperty]
        public Guid FileId { get; set; }
        [JsonProperty]
        public int ChunkNum { get; set; }
        [JsonProperty]
        public int ChunkSize { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        internal byte[] ChunkContent { get; set; }
    }
}
