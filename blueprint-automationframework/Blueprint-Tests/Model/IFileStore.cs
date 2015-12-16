using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IFileStore
    {
        /// <summary>
        /// The list of files added to the file store
        /// </summary>
        List<IFileMetadata> Files { get; }

        /// <summary>
        /// Adds a file to the file store
        /// </summary>
        /// <param name="file">The file being added</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not</param>
        /// <param name="chunkSize">(optional) The chunk size used for POST/PUT requests</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it)</returns>
        IFile AddFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            uint chunkSize = 0, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes a file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        void DeleteFile(string fileId, IUser user, DateTime? expireTime = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The file that was requested</returns>
        IFile GetFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets file metadata from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The metadata for the file that was requested</returns>
        IFileMetadata GetFileMetadata(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the File Store service.
        /// </summary>
        /// <returns>Status of File Store service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        HttpStatusCode GetStatus();
    }
}
