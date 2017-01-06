using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Files;
using File = ServiceLibrary.Models.Files.File;
using FileInfo = ServiceLibrary.Models.Files.FileInfo;

namespace ServiceLibrary.Repositories
{
    public class FileRepository : IFileRepository
    {
        private const string DefaultFileName = "BlueprintFile";
        private const string ContentDispositionHeader = "Content-Disposition";
        private const string ContentTypeHeader = "Content-Type";
        private const string StoredDateHeader = "Stored-Date";
        private const string FileSizeHeader = "File-Size";
        private const string FileChunkCountHeader = "File-Chunk-Count";

        private IHttpWebClient _httpWebClient;

        public FileRepository(IHttpWebClient httpWebClient)
        {
            if (httpWebClient == null)
            {
                throw new ArgumentNullException(nameof(httpWebClient));
            }

            _httpWebClient = httpWebClient;

            // Making calls to FileStore with SSL (HTTPS) requires certificate validation.
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        public async Task<File> GetFileAsync
        (
            Uri baseAddress, 
            Guid fileId, 
            string sessionToken, 
            int timeout = ServiceConstants.DefaultRequestTimeout
        )
        {
            if (baseAddress == null)
            {
                throw new ArgumentNullException(nameof(baseAddress));
            }

            if (string.IsNullOrEmpty(sessionToken))
            {
                throw new AuthenticationException("Authentication is required", ErrorCodes.UnauthorizedAccess);
            }

            return new File
            {
                Info = await GetFileInfoAsync(baseAddress, fileId, sessionToken, timeout),
                ContentStream = await GetFileContentStream(baseAddress, fileId, sessionToken, timeout)
            };
        }

        private bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // This will make it accept all certificates. It's safe because FileStore is an internal service.
            // If it becomes public we must have a valid trusted certificate installed on the server.
            return true;
        }

        private async Task<FileInfo> GetFileInfoAsync(Uri baseAddress, Guid fileId, string sessionToken, int timeout)
        {
            var requestUri = new Uri(baseAddress, string.Format("/svc/filestore/files/{0}", fileId));
            var request = _httpWebClient.CreateHttpWebRequest(requestUri, "Head", sessionToken, timeout);
            var response = await _httpWebClient.GetHttpWebResponseAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new FileInfo
                    {
                        Name = GetFileName(response),
                        Type = response.Headers.GetValue<MediaTypeHeaderValue>(ContentTypeHeader).MediaType,
                        StoredDate = response.Headers.GetValue<DateTime>(StoredDateHeader),
                        Size = response.Headers.GetValue<int>(FileSizeHeader),
                        ChunkCount = response.Headers.GetValue<int>(FileChunkCountHeader)
                    };

                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException
                    (
                        string.Format("File with id {0} is not found", fileId), 
                        ErrorCodes.ResourceNotFound
                    );

                default:
                    throw new Exception
                    (
                        string.Format
                        (
                            "Failed to get file information for id '{0}': Unexpected status code '{1}'", 
                            fileId, 
                            response.StatusCode
                        )
                    );
            }
        }

        private async Task<Stream> GetFileContentStream(Uri baseAddress, Guid fileId, string sessionToken, int timeout)
        {
            var requestUri = new Uri(baseAddress, string.Format("/svc/filestore/files/{0}", fileId));
            var request = _httpWebClient.CreateHttpWebRequest(requestUri, "Get", sessionToken, timeout);
            var response = await _httpWebClient.GetHttpWebResponseAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new NetworkFileStream(response.GetResponseStream(), response.ContentLength);

                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException
                    (
                        string.Format("File with id {0} is not found", fileId), 
                        ErrorCodes.ResourceNotFound
                    );

                default:
                    throw new Exception
                    (
                        string.Format
                        (
                            "Failed to get file content for id '{0}': Unexpected status code '{1}'.", 
                            fileId, 
                            response.StatusCode
                        )
                    );
            }
        }

        private string GetFileName(HttpWebResponse response)
        {
            var fileName = response.Headers.GetValue<ContentDispositionHeaderValue>(ContentDispositionHeader).FileName;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = DefaultFileName;
            }
            else
            {
                //Replace unsupported character " with empty string
                fileName = fileName.Replace("\"", "");

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

                if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
                {
                    var extension = Path.GetExtension(fileName);
                    fileName = DefaultFileName + extension;
                }
            }

            return fileName;
        }
    }
}
