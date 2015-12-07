using System;

namespace Model.Impl
{
    public class File : IFile
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Id { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
