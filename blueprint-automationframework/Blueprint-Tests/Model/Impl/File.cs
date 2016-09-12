using System;
using System.Collections.Generic;

namespace Model.Impl
{
    public class File : IFile
    {
        public IEnumerable<byte> Content { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string Guid { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
