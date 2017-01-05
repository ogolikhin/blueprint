using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Web;
using File = ServiceLibrary.Models.Files.File;
using FileInfo = ServiceLibrary.Models.Files.FileInfo;

namespace ServiceLibrary.Repositories
{
    public class FileRepository : IFileRepository
    {
        private const string DefaultFileName = "BlueprintFile";

        public FileRepository()
        {
            // Making calls to FileStore with SSL (HTTPS) requires certificate validation.
            // This will make it accept all certificates. It's safe because FileStore is an internal service.
            // If it becomes public we must have a valid trusted certificate installed on the server.
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(AcceptAllCertifications);
        }

        public async Task<FileInfo> GetFileInfoAsync(Uri baseUri, Guid fileId, string sessionToken = null, int? timeout = null)
        {
            var uri = new Uri(baseUri, string.Format("/svc/filestore/files/{0}", fileId));
            var request = CreateHttpWebRequest(uri, "Head", sessionToken, timeout);

            using (var response = await GetHttpWebResponseAsync(request))
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return new FileInfo
                        {
                            Name = GetFileName(response),
                            Type = response.Headers.GetWebHeaderValue<MediaTypeHeaderValue>("Content-Type").MediaType,
                            Size = response.Headers.GetWebHeaderValue<int>("File-Size"),
                            StoredDate = response.Headers.GetWebHeaderValue<DateTime>("Stored-Date"),
                            ChunkCount = response.Headers.GetWebHeaderValue<int>("File-Chunk-Count")
                        };

                    case HttpStatusCode.NotFound:
                        return null;

                    default:
                        throw new Exception(string.Format("Failed to get file information for id '{0}': Unexpected status code '{1}'", fileId, response.StatusCode));
                }
            }
        }

        public async Task<File> GetFileAsync(Uri baseUri, Guid fileId, string sessionToken = null)
        {
            var info = await GetFileInfoAsync(baseUri, fileId, sessionToken);
            if (info == null)
            {
                return null;
            }

            var stream = await GetFileContentStream(baseUri, fileId, sessionToken);
            if (stream == null)
            {
                return null;
            }

            return new File
            {
                Info = info,
                ContentStream = stream
            };
        }

        private async Task<Stream> GetFileContentStream(Uri baseUri, Guid fileId, string sessionToken = null, int? timeout = null)
        {
            var uri = new Uri(baseUri, string.Format("/svc/filestore/files/{0}", fileId));
            var request = CreateHttpWebRequest(uri, "Get", sessionToken, timeout);
            var response = await GetHttpWebResponseAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new NetworkFileStream(response.GetResponseStream(), response.ContentLength);

                case HttpStatusCode.NotFound:
                    return null;

                default:
                    throw new Exception(string.Format("Failed to get file content for id '{0}': Unexpected status code '{1}'.", fileId, response.StatusCode));
            }
        }

        private string GetFileName(HttpWebResponse response)
        {
            var fileName = response.Headers.GetWebHeaderValue<ContentDispositionHeaderValue>("Content-Disposition").FileName;

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

        private HttpWebRequest CreateHttpWebRequest(Uri uri, string method, string sessionToken = null, int? timeout = null)
        {
            var request = WebRequest.CreateHttp(uri);

            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers[ServiceConstants.BlueprintSessionTokenKey] = sessionToken;
            }
            else
            {
                request.Headers[ServiceConstants.BlueprintSessionIgnoreKey] = string.Empty;
            }
            
            request.Method = method;
            request.Timeout = timeout.HasValue ? timeout.Value : ServiceConstants.DefaultHttpWebRequestTimeout;

            return request;
        }

        private async Task<HttpWebResponse> GetHttpWebResponseAsync(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    // HttpWebRequest that gets created has ConnectionLimit = 2 by default when calling from remote host.
                    throw new Exception
                    (
                        string.Format
                        (
                            "Timeout exception occured. Current connections: {0}, Connection limit: {1}\n{2}",
                            request.ServicePoint.CurrentConnections,
                            request.ServicePoint.ConnectionLimit,
                            ex
                        )
                    );
                }

                var exceptionResponse = ex.Response as HttpWebResponse;
                if (exceptionResponse != null)
                {
                    response = exceptionResponse;
                }
                else
                {
                    throw;
                }
            }

            return response;
        }

        private bool AcceptAllCertifications(object sender, X509Certificate certification, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
