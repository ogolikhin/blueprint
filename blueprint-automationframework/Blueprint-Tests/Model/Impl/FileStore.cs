using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Web;
using Common;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;
using Model.NovaModel;
using Model.NovaModel.Impl;
using System.Globalization;
using Model.Factories;
using System.Data.SqlClient;

namespace Model.Impl
{
    public class FileStore : NovaServiceBase, IFileStore
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";
        private IUser _user = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the FileStore.</param>
        public FileStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Inherited from IFileStore

        /// <seealso cref="IFileStore.Files"/>
        public List<IFileMetadata> Files { get; } = new List<IFileMetadata>();

        public List<INovaFileMetadata> NovaFiles { get; } = new List<INovaFileMetadata>();

        /// <seealso cref="IFileStore.AddFile(IFile, IUser, DateTime?, bool, uint, List{HttpStatusCode}, bool)"/>
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

        /// <seealso cref="IFileStore.GetFile(string, IUser, List{HttpStatusCode}, bool)"/>
        public IFile GetFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return GetFile(fileId, user, RestRequestMethod.GET, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <seealso cref="IFileStore.GetFileMetadata(string, IUser, List{HttpStatusCode}, bool)"/>
        public IFileMetadata GetFileMetadata(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return GetFile(fileId, user, RestRequestMethod.HEAD, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <seealso cref="IFileStore.DeleteFile(string, IUser, DateTime?, List{HttpStatusCode}, bool)"/>
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.FILES_id_, fileId);
            var restApi = new RestApiFacade(Address, tokenValue);

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
                    Files.Remove(Files.First(i => i.Guid == fileId));
                }
            }
        }

        /// <seealso cref="IFileStore.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.FileStore.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.FileStore.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.PostFile(IFile, IUser, DateTime?, bool, List{HttpStatusCode}, bool)"/>
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
                tokenValue = BlueprintToken.NO_TOKEN;
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

            var path = RestPaths.Svc.FileStore.FILES;
            var restApi = new RestApiFacade(Address, tokenValue);

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

            file.Guid = response.Content.Replace("\"", "");

            Files.Add(file);

            // We'll use this user in Dispose() to delete the files.
            if (_user == null)
            {
                _user = user;
            }

            return file;
        }

        /// <seealso cref="IFileStore.PutFile(IFile, byte[], IUser, bool, List{HttpStatusCode}, bool)"/>
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
                tokenValue = BlueprintToken.NO_TOKEN;
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

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.FILES_id_, file.Guid);
            var restApi = new RestApiFacade(Address, tokenValue);

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

        /// <summary>
        /// Gets a file from FileStore.
        /// </summary>
        /// <param name="fileId">The GUID of the file to get.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="webRequestMethod">The request method (GET, HEAD...).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.OK.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header.</param>
        /// <returns>The file that was requested.</returns>
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var restApi = new RestApiFacade(Address, tokenValue);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.FILES_id_, fileId);

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
                    Guid = fileId,
                    LastModifiedDate =
                        DateTime.ParseExact(response.Headers.First(h => h.Key == "Stored-Date").Value.ToString(), "o",
                            null),
                    FileType = response.ContentType,
                    FileName = filename
                };
            }

            return file;
        }

        /// <seealso cref="INovaFile.AddFile(INovaFile, IUser, DateTime?, bool, uint, List{HttpStatusCode}, bool)"/>
        public INovaFile AddFile(INovaFile file,
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
            INovaFile postedFile = PostFile(file, user, expireTime, useMultiPartMime, expectedStatusCodes, sendAuthorizationAsCookie);

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

            }
            return postedFile;
        }

        /// <seealso cref="INovaFile.PostFile(INovaFile, IUser, DateTime?, bool, List{HttpStatusCode}, bool)"/>
        public INovaFile PostFile(INovaFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(user, nameof(user));

            var queryParameters = new Dictionary<string, string>();

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {
                additionalHeaders.Add("FileName", I18NHelper.FormatInvariant("{0}",HttpUtility.UrlEncode(file.FileName, System.Text.Encoding.UTF8)));
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            // TODO: Investigate why passing valid expiredDate to this POST FileStore call doesn't set exiredDate colmn value on Files DB table
            var path = expireTime.HasValue ?
                I18NHelper.FormatInvariant("{0}/?expired={1}.333Z", RestPaths.Svc.FileStore.NOVAFILES,expireTime.Value.ToStringInvariant("o").Remove(expireTime.Value.ToStringInvariant().Length)) 
                : RestPaths.Svc.FileStore.NOVAFILES;
            var restApi = new RestApiFacade(Address, tokenValue);

            // TODO: ask DEV which determine HTTP request content type. FileType?

            var response = restApi.SendRequestAndGetResponse(
                resourcePath: path,
                method: RestRequestMethod.POST,
                fileName: file.FileName,
                fileContent: file.Content.ToArray(),
                contentType: file.FileType,
                useMultiPartMime: useMultiPartMime,
                additionalHeaders: additionalHeaders,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            file.ContentLength = restApi.ReqContentLength;

            var deserialzedResultContent = Deserialization.DeserializeObject<NovaFile>(response.Content);

            file.Guid = deserialzedResultContent.Guid;
            file.UriToFile = deserialzedResultContent.UriToFile;

            NovaFiles.Add(file);

            // We'll use this user in Dispose() to delete the files.
            if (_user == null)
            {
                _user = user;
            }

            return file;
        }

        /// <seealso cref="IFileStore.PutFile(INovaFile, byte[], IUser, bool, List{HttpStatusCode}, bool)"/>
        public INovaFile PutFile(INovaFile file, byte[] chunk, IUser user, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(file, nameof(file));
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {
                additionalHeaders.Add("FileName", I18NHelper.FormatInvariant("{0}", HttpUtility.UrlEncode(file.FileName, System.Text.Encoding.UTF8)));
            }

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.NOVAFILE_id_, file.Guid);
            var restApi = new RestApiFacade(Address, tokenValue);

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

        /// <seealso cref="IFileStore.GetNovaFile(string, IUser, List{HttpStatusCode}, bool)"/>
        public INovaFile GetNovaFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return GetNovaFile(fileId, user, RestRequestMethod.GET, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Gets a file from FileStore.
        /// </summary>
        /// <param name="fileId">The GUID of the file to get.</param>
        /// <param name="user">The user credentials for the request.</param>
        /// <param name="webRequestMethod">The request method (GET, HEAD...).</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  Defaults to HttpStatusCode.OK.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header.</param>
        /// <returns>The file that was requested.</returns>
        private INovaFile GetNovaFile(string fileId,
            IUser user,
            RestRequestMethod webRequestMethod,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(fileId, nameof(fileId));
            ThrowIf.ArgumentNull(user, nameof(user));

            NovaFile file = null;

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var restApi = new RestApiFacade(Address, tokenValue);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.NOVAFILE_id_, fileId);

            var response = restApi.SendRequestAndGetResponse(
                path,
                webRequestMethod,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            if (webRequestMethod == RestRequestMethod.HEAD)
            {
                Assert.That(!response.RawBytes.Any(), "Content returned for a HEAD request!");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string filename = HttpUtility.UrlDecode(new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName);
                file = new NovaFile
                {
                    Content = response.RawBytes.ToArray(),
                    Guid = fileId,
                    LastModifiedDate = DateTime.SpecifyKind(DateTime.Parse(response.Headers.First(h => h.Key.Equals("Date")).Value.ToString(), CultureInfo.InvariantCulture), DateTimeKind.Local),
                    FileType = response.ContentType,
                    FileName = filename,
                    ContentLength = response.RawBytes.ToArray().LongLength
                };
            }

            return file;
        }

        /// <seealso cref="IFileStore.DeleteNovaFile(string, IUser, DateTime?, List{HttpStatusCode}, bool)"/>
        public void DeleteNovaFile(string fileId,
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
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.FileStore.FILES_id_, fileId);
            var restApi = new RestApiFacade(Address, tokenValue);

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
                    NovaFiles.Remove(NovaFiles.First(i => i.Guid == fileId));
                }
            }
        }

        /// <seealso cref="IFileStore.GetSQLExpiredTime(string)"/>
        public DateTime? GetSQLExpiredTime(string fileId)
        {
            string FILES_TABLE = "[FileStore].[Files]";
            string query = I18NHelper.FormatInvariant("SELECT ExpiredTime FROM {0} WHERE FileId = '{1}'", FILES_TABLE, fileId);
            DateTime? expiredTime = null;

            using (IDatabase database = DatabaseFactory.CreateDatabase(databaseName: "FileStore"))
            {
                database.Open();
                Logger.WriteDebug("Running: {0}", query);
                try
                {
                    using (SqlCommand cmd = database.CreateSqlCommand(query))
                    using (var sqlDataReader = cmd.ExecuteReader())
                    {
                        if (sqlDataReader.HasRows)
                        {
                            while (sqlDataReader.Read())
                            {
                                int expiredTimeOrdinal = sqlDataReader.GetOrdinal("ExpiredTime");
                                var sqlExpiredTime = sqlDataReader.GetSqlDateTime(expiredTimeOrdinal);
                                if (sqlExpiredTime.IsNull)
                                {
                                    return null;
                                }
                                   
                                else
                                {
                                    expiredTime = (DateTime)(sqlExpiredTime);
                                }
                            }
                        }
                        else
                        {
                            throw new SqlQueryFailedException(I18NHelper.FormatInvariant("No rows were inserted when running: {0}", query));
                        }
                    }
                }
                catch
                {
                    throw;
                }
            }

            return expiredTime;
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
                    DeleteFile(file.Guid, _user);
                }

                if (NovaFiles.Count > 0)
                {
                    Assert.NotNull(_user,
                        "It shouldn't be possible for the '{0}' member variable to be null if files were added to Filestore!", nameof(_user));
                }

                // Delete all the files that were created.
                foreach (var novaFile in NovaFiles.ToArray())
                {
                    DeleteNovaFile(novaFile.Guid, _user);
                }

                Files.Clear();

                NovaFiles.Clear();
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
