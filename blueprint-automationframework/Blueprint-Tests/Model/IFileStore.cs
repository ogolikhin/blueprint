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
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.Created.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it)</returns>
        IFile AddFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            uint chunkSize = 0, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Deletes a file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        void DeleteFile(string fileId, IUser user, DateTime? expireTime = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets a file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was requested</returns>
        IFile GetFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets file metadata from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The metadata for the file that was requested</returns>
        IFileMetadata GetFileMetadata(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets the current status of the File Store service.
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Status of File Store service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        HttpStatusCode GetStatus(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Adds a file to the file store with a POST command only.
        /// </summary>
        /// <param name="file">The file being added.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted.</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.Created.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        IFile PostFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Appends a file to the file store with the PUT command only.
        /// </summary>
        /// <param name="file">The file being appended.</param>
        /// <param name="chunk">The file data chunk to PUT into FileStore.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.OK.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        IFile PutFile(IFile file, byte[] chunk, IUser user, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
