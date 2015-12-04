using System;

namespace Model
{
    public interface IFileMetadata
    {
        string FileName { get; set; }
        string FileType { get; set; }
        Guid Id { get; set; }
        DateTime LastModifiedDate { get; set; }
    }


    public interface IFile : IFileMetadata
    {
        /// <summary>
        /// The file data.
        /// </summary>
        byte[] Content { get; set; }
    }
}
