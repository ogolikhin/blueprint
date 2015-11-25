using System;

namespace FileStore.Repositories
{
    public interface IContentReadStream : IDisposable
    {
        /// <summary>
        ///
        /// </summary>
        long Length { get; }

        /// <summary>
        ///
        /// </summary>
        string FileType { get; }

        /// <summary>
        ///
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Determines if the file exists in the legacy filestream database
        /// </summary>
        bool FileExists { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="fileGuid"></param>
        void Setup(string connectionString, Guid fileGuid);
    }
}
