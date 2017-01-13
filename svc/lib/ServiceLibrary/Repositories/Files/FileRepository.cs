using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Files;
using File = ServiceLibrary.Models.Files.File;
using FileInfo = ServiceLibrary.Models.Files.FileInfo;

namespace ServiceLibrary.Repositories.Files
{
    public class FileRepository : IFileRepository
    {
        private const string DefaultFileName = "BlueprintFile";

        private readonly IHttpWebClient _httpWebClient;

        public FileRepository(IHttpWebClient httpWebClient)
        {
            if (httpWebClient == null)
            {
                throw new ArgumentNullException(nameof(httpWebClient));
            }

            _httpWebClient = httpWebClient;
        }

        public async Task<File> GetFileAsync(Guid fileId)
        {
            return new File
            {
                Info = await GetFileInfoAsync(fileId),
                ContentStream = await GetFileContentStreamAsync(fileId)
            };
        }

        private async Task<FileInfo> GetFileInfoAsync(Guid fileId)
        {
            var requestAddress = $"/svc/filestore/files/{fileId}";
            var request = _httpWebClient.CreateHttpWebRequest(requestAddress, "Head");
            var response = await _httpWebClient.GetHttpWebResponseAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new FileInfo
                    {
                        Name = GetFileName(response),
                        Type = response.Headers.GetValue<MediaTypeHeaderValue>(ServiceConstants.ContentTypeHeader).MediaType,
                        StoredDate = response.Headers.GetValue<DateTime>(ServiceConstants.StoredDateHeader),
                        Size = response.Headers.GetValue<int>(ServiceConstants.FileSizeHeader),
                        ChunkCount = response.Headers.GetValue<int>(ServiceConstants.FileChunkCountHeader)
                    };

                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Authentication is required", ErrorCodes.UnauthorizedAccess);

                case HttpStatusCode.NotFound:
                case HttpStatusCode.ServiceUnavailable:
                    throw new ResourceNotFoundException($"File with id {fileId} is not found", ErrorCodes.ResourceNotFound);

                default:
                    throw new Exception($"Failed to get file information for id '{fileId}': Unexpected status code '{response.StatusCode}'");
            }
        }

        private async Task<Stream> GetFileContentStreamAsync(Guid fileId)
        {
            var requestAddress = $"/svc/filestore/files/{fileId}";
            var request = _httpWebClient.CreateHttpWebRequest(requestAddress, "Get");
            var response = await _httpWebClient.GetHttpWebResponseAsync(request);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new NetworkFileStream(response.GetResponseStream(), response.ContentLength);

                case HttpStatusCode.Unauthorized:
                    throw new AuthenticationException("Authentication is required", ErrorCodes.UnauthorizedAccess);

                case HttpStatusCode.NotFound:
                case HttpStatusCode.ServiceUnavailable:
                    throw new ResourceNotFoundException($"File with id {fileId} is not found", ErrorCodes.ResourceNotFound);

                default:
                    throw new Exception($"Failed to get file content for id '{fileId}': Unexpected status code '{response.StatusCode}'.");
            }
        }

        private static string GetFileName(HttpWebResponse response)
        {
            var fileName = response.Headers.GetValue<ContentDispositionHeaderValue>(ServiceConstants.ContentDispositionHeader).FileName;

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
