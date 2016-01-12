using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using NUnit.Framework;
using Utilities.Facades;

namespace Model.Impl
{
    public class FileStore : IFileStore
    {
        private const string SVC_PATH = "svc/filestore";

        private static string _address;

        #region Inherited from IFileStore

        public List<IFileMetadata> Files { get; } = new List<IFileMetadata>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the FileStore.</param>
        public FileStore(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            _address = address;
        }

        /// <seealso cref="IFileStore.AddFile"/>
        public IFile AddFile(IFile file,
            IUser user,
            DateTime? expireTime = null,
            bool useMultiPartMime = false,
            uint chunkSize = 0,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (file == null) { throw new ArgumentNullException(nameof(file)); }

            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            byte[] fileBytes = file.Content;
            byte[] chunk = fileBytes;

            // If we are chunking the file, get the first chunk ready.
            if (chunkSize > 0 && fileBytes.Length > chunkSize)
            {
                chunk = fileBytes.Take((int)chunkSize).ToArray();
                file.Content = chunk;
            }

            // Post the first chunk of the file.
            IFile postedFile = PostFile(file, user, expireTime, useMultiPartMime, expectedStatusCodes);

            if (chunkSize > 0 && fileBytes.Length > chunkSize)
            {
                List<byte> fileChunkList = new List<byte>(postedFile.Content);
                byte[] rem = fileBytes.Skip((int)chunkSize).ToArray();

                do
                {
                    chunk = rem.Take((int)chunkSize).ToArray();
                    rem = rem.Skip((int)chunkSize).ToArray();

                    // Put each subsequent chunk of the file.
                    PutFile(file, chunk, user, useMultiPartMime, expectedStatusCodes);
                    fileChunkList.AddRange(chunk);
                } while (rem.Length > 0);

                postedFile.Content = fileChunkList.ToArray();
            }

            return postedFile;
        }

        /// <seealso cref="IFileStore.GetFile"/>
        public IFile GetFile(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetFile(fileId, user, RestRequestMethod.GET, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.GetFileMetadata"/>
        public IFileMetadata GetFileMetadata(string fileId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetFile(fileId, user, RestRequestMethod.HEAD, expectedStatusCodes);
        }

        /// <seealso cref="IFileStore.DeleteFile"/>
        public void DeleteFile(string fileId,
            IUser user,
            DateTime? expireTime = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (fileId == null)
            {
                throw new ArgumentNullException(nameof(fileId));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var queryParameters = new Dictionary<string, string>();

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToString("o"));
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            var path = string.Format("{0}/files/{1}", SVC_PATH, fileId);
            var restApi = new RestApiFacade(_address, user.Username, user.Password, user.Token?.AccessControlToken);

            try
            {
                restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
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
        public HttpStatusCode GetStatus()
        {
            var restApi = new RestApiFacade(_address, token:string.Empty);
            var path = string.Format("{0}/status", SVC_PATH);

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET);

            return response.StatusCode;
        }

        /// <seealso cref="IFileStore.PostFile"/>
        public IFile PostFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (file == null) { throw new ArgumentNullException(nameof(file)); }

            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            var queryParameters = new Dictionary<string, string>();

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToString("o"));
            }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {

                additionalHeaders.Add("Content-Disposition", string.Format("form-data; name=attachment; filename={0}", file.FileName));
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var path = string.Format("{0}/files", SVC_PATH);
            var restApi = new RestApiFacade(_address, user.Username, user.Password, user.Token?.AccessControlToken);
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, file.FileName, file.Content, file.FileType, useMultiPartMime, additionalHeaders, queryParameters, expectedStatusCodes);

            file.Id = response.Content.Replace("\"", "");

            Files.Add(file);

            return file;
        }

        /// <seealso cref="IFileStore.PutFile"/>
        public IFile PutFile(IFile file, byte[] chunk, IUser user, bool useMultiPartMime = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (file == null) { throw new ArgumentNullException(nameof(file)); }

            if (user == null) { throw new ArgumentNullException(nameof(user)); }

            var additionalHeaders = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(file.FileType))
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            if (!string.IsNullOrEmpty(file.FileName))
            {

                additionalHeaders.Add("Content-Disposition", string.Format("form-data; name=attachment; filename={0}", file.FileName));
            }

            var path = string.Format("{0}/files/{1}", SVC_PATH, file.Id);
            var restApi = new RestApiFacade(_address, user.Username, user.Password, user.Token?.AccessControlToken);
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.PUT, file.FileName, chunk, file.FileType, useMultiPartMime, additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            return file;
        }

        private static IFile GetFile(string fileId,
            IUser user,
            RestRequestMethod webRequestMethod,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            File file = null;

            if (fileId == null)
            {
                throw new ArgumentNullException(nameof(fileId));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password, user.Token?.AccessControlToken);
            var path = string.Format("{0}/files/{1}", SVC_PATH, fileId);

            var response = restApi.SendRequestAndGetResponse(path, webRequestMethod, expectedStatusCodes:expectedStatusCodes);

            if (webRequestMethod == RestRequestMethod.HEAD)
            {
                Assert.That(response.RawBytes.Length == 0, "Content returned for a HEAD request!");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes,
                    Id = fileId,
                    LastModifiedDate =
                        DateTime.ParseExact(response.Headers.First(h => h.Key == "Stored-Date").Value.ToString(), "o",
                            null),
                    FileType = response.ContentType,
                    FileName =
                        new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName
                };
            }

            return file;
        }

        #endregion Inherited from IFileStore
    }
}
