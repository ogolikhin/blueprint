using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web;
using Common;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class FileStore : NovaServiceBase, IFileStore
    {
        private const string SVC_PATH = "svc/filestore";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";
        private IUser _user = null;

        #region Inherited from IFileStore

        public List<IFileMetadata> Files { get; } = new List<IFileMetadata>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the FileStore.</param>
        public FileStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        /// <seealso cref="IFileStore.AddFile"/>
        public IFile AddFile(IFile file,
            IUser user,
            DateTime? expireTime = null,
            bool useMultiPartMime = false,
            uint chunkSize = 0,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(user, nameof(user));

            byte[] fileBytes = file.Content.ToArray();
            byte[] chunk = fileBytes;

            // If we are chunking the file, get the first chunk ready.
            if (chunkSize > 0 && fileBytes.Length > chunkSize)
            {
                chunk = fileBytes.Take((int)chunkSize).ToArray();
                file.Content = chunk;
            }

            // Post the first chunk of the file.
            IFile postedFile = PostFile(file, user, expireTime, useMultiPartMime, expectedStatusCodes, sendAuthorizationAsCookie);

            if (chunkSize > 0 && fileBytes.Length > chunkSize)
            {
                List<byte> fileChunkList = new List<byte>(postedFile.Content);
                byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();

                do
                {
                    chunk = rem.Take((int)chunkSize).ToArray();
                    rem = rem.Skip((int)chunkSize).ToArray();

                    // Put each subsequent chunk of the file.
                    PutFile(file, chunk, user, useMultiPartMime, expectedStatusCodes, sendAuthorizationAsCookie);
                    fileChunkList.AddRange(chunk);
                } while (rem.Length > 0);

                postedFile.Content = fileChunkList.ToArray();
            }

            return postedFile;
        }

        /// <seealso cref="IFileStore.GetFile"/>
        public IFile GetFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return GetFile(fileId, user, RestRequestMethod.GET, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <seealso cref="IFileStore.GetFileMetadata"/>
        public IFileMetadata GetFileMetadata(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return GetFile(fileId, user, RestRequestMethod.HEAD, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <seealso cref="IFileStore.DeleteFile"/>
        public void DeleteFile(string fileId,
            IUser user,
            DateTime? expireTime = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(fileId, nameof(fileId));
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParameters = new Dictionary<string, string>();

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToStringInvariant("o"));
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            var path = I18NHelper.FormatInvariant("{0}/files/{1}", SVC_PATH, fileId);
            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            try
            {
                restApi.SendRequestAndGetResponse(
                    path, 
                    RestRequestMethod.DELETE, 
                    queryParameters: queryParameters, 
                    expectedStatusCodes: expectedStatusCodes, 
                    cookies: cookies);
            }
            finally
            {
                if (expireTime == null || expireTime <= DateTime.Now)
                {
                    Files.Remove(Files.First(i => i.Id == fileId));
                }
            }
        }

        /// <seealso cref="IFileStore.GetStatus"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(SVC_PATH, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.GetStatusUpcheck"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(SVC_PATH, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.PostFile"/>
        public IFile PostFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParameters = new Dictionary<string, string>();

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToStringInvariant("o"));
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {
                additionalHeaders.Add("Content-Disposition",
                    I18NHelper.FormatInvariant("form-data; name=attachment; filename=\"{0}\"",
                        HttpUtility.UrlEncode(file.FileName, System.Text.Encoding.UTF8)));
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var path = I18NHelper.FormatInvariant("{0}/files", SVC_PATH);

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);
            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                file.FileName,
                file.Content.ToArray(),
                file.FileType,
                useMultiPartMime,
                additionalHeaders,
                queryParameters,
                expectedStatusCodes,
                cookies);

            file.Id = response.Content.Replace("\"", "");

            Files.Add(file);

            // We'll use this user in Dispose() to delete the files.
            if (_user == null)
            {
                _user = user;
            }

            return file;
        }

        /// <seealso cref="IFileStore.PutFile"/>
        public IFile PutFile(IFile file, byte[] chunk, IUser user, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {
                additionalHeaders.Add("Content-Disposition",
                    I18NHelper.FormatInvariant("form-data; name=attachment; filename=\"{0}\"",
                        HttpUtility.UrlEncode(file.FileName, System.Text.Encoding.UTF8)));
            }

            var path = I18NHelper.FormatInvariant("{0}/files/{1}", SVC_PATH, file.Id);
            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);
            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PUT,
                file.FileName,
                chunk,
                file.FileType,
                useMultiPartMime,
                additionalHeaders,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return file;
        }

        private IFile GetFile(string fileId,
            IUser user,
            RestRequestMethod webRequestMethod,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(fileId, nameof(fileId));
            ThrowIf.ArgumentNull(user, nameof(user));

            File file = null;

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);
            var path = I18NHelper.FormatInvariant("{0}/files/{1}", SVC_PATH, fileId);

            var response = restApi.SendRequestAndGetResponse(
                path, 
                webRequestMethod, 
                expectedStatusCodes:expectedStatusCodes, 
                cookies: cookies);

            if (webRequestMethod == RestRequestMethod.HEAD)
            {
                Assert.That(!response.RawBytes.Any(), "Content returned for a HEAD request!");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string filename = HttpUtility.UrlDecode(new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName);

                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    Id = fileId,
                    LastModifiedDate =
                        DateTime.ParseExact(response.Headers.First(h => h.Key == "Stored-Date").Value.ToString(), "o",
                            null),
                    FileType = response.ContentType,
                    FileName = filename
                };
            }

            return file;
        }

        #endregion Inherited from IFileStore

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all files that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(FileStore), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Logger.WriteDebug("Deleting all files created by this FileStore instance...");

                if (Files.Count > 0)
                {
                    Assert.NotNull(_user,
                        "It shouldn't be possible for the '{0}' member variable to be null if files were added to Filestore!", nameof(_user));
                }

                // Delete all the files that were created.
                foreach (var file in Files.ToArray())
                {
                    DeleteFile(file.Id, _user);
                }

                Files.Clear();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all sessions that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
