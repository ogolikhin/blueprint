using System;

namespace ServiceLibrary.Models.Files
{
    public sealed class FileInfo
    {
        public string Name { get; internal set; }

        public string Type { get; internal set; }

        public long Size { get; internal set; }

        public DateTime StoredDate { get; internal set; }

        public int ChunkCount { get; internal set; }
    }
}
