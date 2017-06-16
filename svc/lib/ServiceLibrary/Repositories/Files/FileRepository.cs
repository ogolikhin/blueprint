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
using System.Web;
using System.Xml;

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

        public async Task<string> UploadFileAsync(string fileName, string fileType, Stream content, DateTime? expired = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            var request = CreateUploadFileRequest(fileName, fileType, content, expired);

            var response = await _httpWebClient.GetHttpWebResponseAsync(request);

            var fileGuid = GetFileGuidFromResponse(response);

            return fileGuid;
        }

        private HttpWebRequest CreateUploadFileRequest(string fileName, string fileType, Stream content, DateTime? expired = null)
        {
            string expireDate = expired == null ? null : 
                "?expired=" + HttpUtility.UrlEncode(expired.Value.ToStringInvariant("o"));
            var requestAddress = I18NHelper.FormatInvariant("/svc/components/filestore/files/{0}/{1}", fileName, expireDate);

            var request = _httpWebClient.CreateHttpWebRequest(requestAddress, "POST");
            request.ContentType = MimeMapping.GetMimeMapping(string.IsNullOrWhiteSpace(fileType) ? fileName : fileType);
            request.Accept = "application/xml";

            using (var requestStream = request.GetRequestStream())
            {
                content.CopyTo(requestStream);
            }

            return request;
        }

        private string GetFileGuidFromResponse(HttpWebResponse response)
        {
            string fileGuid = null;            
            string xmlReply;
            
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                xmlReply = streamReader.ReadToEnd();
            }

            if (response.StatusCode == HttpStatusCode.Created)
            {
                XmlDocument dox = new XmlDocument();
                dox.LoadXml(xmlReply);
                var nodes = dox.GetElementsByTagName("Guid");
                fileGuid = nodes.Count > 0 ? nodes[0].InnerText : null;                
            }
            else
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new AuthenticationException("Authentication is required.", ErrorCodes.UnauthorizedAccess);

                    case HttpStatusCode.BadRequest:
                        throw new BadRequestException("Invalid request.", ErrorCodes.BadRequest);

                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.ServiceUnavailable:
                        throw new ResourceNotFoundException("Resource not found.", ErrorCodes.ResourceNotFound);

                    default:
                        throw new Exception(I18NHelper.FormatInvariant("Failed to upload file. Unexpected status code '{0}'", response.StatusCode));
                }
            }

            return fileGuid;
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
