using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Logging;
using Model.Facades;
using RestSharp.Extensions;
using Utilities.Facades;
using Utilities.Factories;

namespace Model.Impl
{
    public class FileStore : IFileStore
    {
        private const string SVC_PATH = "svc/filestore";

        private static string _address;
        private List<IFileMetadata> _Files = new List<IFileMetadata>();

        #region Inherited from IFileStore

        public List<IFileMetadata> Files { get; } = new List<IFileMetadata>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the FileStore.</param>
        public FileStore(string address)
        {
            if (address == null) { throw new ArgumentNullException(nameof(address)); }

            _address = address;


        }

        public IFile AddFile(IFile file, IUser user, DateTime? expireTime = null, bool useMultiPartMime = false, int chunkSize = 0, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var queryParameters = new Dictionary<string, string>();
            var additionalHeaders = new Dictionary<string, string>();

            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password);

            var path = string.Format("{0}/files", SVC_PATH);

            var fileBytes = file.Content.ToArray();
            var chunk = fileBytes;

            if (chunkSize > 0)
            {
                 chunk = fileBytes.Take(chunkSize).ToArray();
            }

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToString("o"));
            }

            if (file.FileType.HasValue())
            {
                additionalHeaders.Add("Content-Type", file.FileType);
            }

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, file.FileName, chunk, useMultiPartMime, additionalHeaders, queryParameters, new List<HttpStatusCode> { HttpStatusCode.OK});

            file.Id = response.Content.Replace("\"", "");

            if (chunkSize > 0)
            {
                path = string.Format("{0}/files/{1}", SVC_PATH, file.Id);

                var rem = fileBytes.Skip(chunkSize).ToArray();

                while (rem.Length > 0 && response.StatusCode == HttpStatusCode.OK)
                {
                    chunk = rem.Take(chunkSize).ToArray();
                    rem = rem.Skip(chunkSize).ToArray();
                    response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.PUT, file.FileName, chunk, useMultiPartMime, additionalHeaders, queryParameters, new List<HttpStatusCode> { HttpStatusCode.OK });
                }
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            if (!expectedStatusCodes.Contains(response.StatusCode))
            {
                throw WebExceptionFactory.Create((int)response.StatusCode);
            } 

            return file;
        }

        public IFile GetFile(string fileId, IUser user, HttpStatusCode? expectedStatusCode = null)
        {
            return GetFile(fileId, user, RestRequestMethod.GET, expectedStatusCode);
        }

        public IFileMetadata GetFileMetadata(string fileId, IUser user, HttpStatusCode? expectedStatusCode = null)
        {
            return GetFile(fileId, user, RestRequestMethod.GET, expectedStatusCode);
        }
        public void DeleteFile(string fileId, IUser user, DateTime? expireTime = null, HttpStatusCode? expectedStatusCode = null)
        {
            var queryParameters = new Dictionary<string, string>();

            if (!fileId.HasValue())
            {
                throw new ArgumentNullException(nameof(fileId));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password);
            var path = string.Format("{0}/files/{1}", SVC_PATH, fileId);

            if (expireTime.HasValue)
            {
                queryParameters.Add("expired", expireTime.Value.ToString("o"));
            }

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE, queryParameters: queryParameters);

            if (!expectedStatusCode.HasValue)
            {
                expectedStatusCode = HttpStatusCode.OK;
            }

            if (expectedStatusCode != response.StatusCode)
            {
                throw WebExceptionFactory.Create((int)response.StatusCode);
            }
        }

        public HttpStatusCode GetStatus()
        {
            var restApi = new RestApiFacade(_address);
            var path = string.Format("{0}/status", SVC_PATH);

            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET);

            return response.StatusCode;
        }

        private static IFile GetFile(string fileId, IUser user, RestRequestMethod webRequestMethod, HttpStatusCode? expectedStatusCode = null)
        {
            if (fileId == null)
            {
                throw new ArgumentNullException(nameof(fileId));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var restApi = new RestApiFacade(_address, user.Username, user.Password);
            var path = string.Format("{0}/files/{1}", SVC_PATH, fileId);

            var response = restApi.SendRequestAndGetResponse(path, webRequestMethod);

            if (!expectedStatusCode.HasValue)
            {
                expectedStatusCode = HttpStatusCode.OK;
            }

            if (expectedStatusCode != response.StatusCode)
            {
                throw WebExceptionFactory.Create((int)response.StatusCode);
            }

            if (expectedStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var file = new File
            {
                Content = response.RawBytes,
                Id = fileId,
                LastModifiedDate = DateTime.ParseExact(response.Headers.First(h => h.Key == "Stored-Date").Value.ToString(), "o", null),
                FileType = response.ContentType,
                FileName = new ContentDisposition(response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName
            };

            return file;
        }

        #endregion Inherited from IFileStore
    }
}
