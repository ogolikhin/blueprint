using Model.NovaModel;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IFileStore : IDisposable
    {
        /// <summary>
        /// The URL of the FileStore server (i.e. the part before the /svc/filestore).
        /// </summary>
        string Address { get; }

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
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the File Store service.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Status of File Store service.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

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

        /// <summary>
        /// Adds a Nova file to the file store
        /// </summary>
        /// <param name="file">The file being added</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not</param>
        /// <param name="chunkSize">(optional) The chunk size used for POST/PUT requests</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.Created.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it)</returns>
        INovaFile AddNovaFile(INovaFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            uint chunkSize = 0, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Adds a Nova file to the file store with a POST command only.
        /// </summary>
        /// <param name="file">The file being added.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted.</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.Created.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        INovaFile PostNovaFile(INovaFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Appends a Nova file to the file store with the PUT command only.
        /// </summary>
        /// <param name="file">The file being appended.</param>
        /// <param name="chunk">The file data chunk to PUT into FileStore.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="useMultiPartMime">(optional) Flag to use multi-part mime or not.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.OK.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was added (including the file ID that FileStore gave it).</returns>
        INovaFile PutNovaFile(INovaFile file, byte[] chunk, IUser user, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets a Nova file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The file that was requested</returns>
        INovaFile GetNovaFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Deletes a Nova file from the file store
        /// </summary>
        /// <param name="fileId">The file GUID</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expireTime">(optional) The file expiry date/time; The time after which the file can be deleted</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        void DeleteNovaFile(string fileId, IUser user, DateTime? expireTime = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

    }
}
