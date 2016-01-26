using System;
using System.Collections.Generic;

namespace Model
{
    public interface IFileMetadata
    {
        string FileName { get; set; }
        string FileType { get; set; }
        string Id { get; set; }
        DateTime LastModifiedDate { get; set; }
    }


    public interface IFile : IFileMetadata
    {
        /// <summary>
        /// The file data.
        /// </summary>
        IEnumerable<byte> Content { get; set; }
    }
}
