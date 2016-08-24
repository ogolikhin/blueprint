using FileStore.Helpers;
using FileStore.Models;
using FileStore.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace FileStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("files")]
    public class FilesController : ApiController
    {

        //remove unnecessary headers from web api
        //http://www.4guysfromrolla.com/articles/120209-1.aspx

        private readonly IFilesRepository _filesRepo;
        private readonly IFileStreamRepository _fileStreamRepo;
        private readonly IConfigRepository _configRepo;
        private readonly sl.IServiceLogRepository _log;

        private const string StoredDate = "Stored-Date";
        private const string FileSize = "File-Size";
        private const string FileChunkCount = "File-Chunk-Count";
        private const string Attachment = "attachment";
        private const string StoredDateFormat = "o";

        public FilesController() : this(new SqlFilesRepository(), new FileStreamRepository(), ConfigRepository.Instance, new sl.ServiceLogRepository())
        {
        }

        internal FilesController(IFilesRepository fr, IFileStreamRepository fsr, IConfigRepository cr, sl.IServiceLogRepository log)
        {
            _filesRepo = fr;
            _fileStreamRepo = fsr;
            _configRepo = cr;
            _log = log;
        }

        #region Service Methods

        /// <summary>
        /// GetFileHead
        /// </summary>
        /// <remarks>
        /// Returns information about a file including name, type and size.
        /// </remarks>
        /// <param name="id">The GUID of the file.</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The id is missing or malformed.</response>
        /// <response code="404">Not Found. The file does not exist or has expired.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpHead, NoCache]
        [Route("{id}"), SessionRequired]
        public async Task<IHttpActionResult> GetFileHead(string id)
        {
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"HEAD:{id}, Getting file head");
            Models.File file;

            try
            {
                var fileId = Models.File.ConvertToStoreId(id);

                file = await _filesRepo.GetFileHead(fileId);

                if (file == null && _fileStreamRepo.FileExists(fileId))
                {
                    // if the file is not found in the FileStore check the
                    // legacy database for the file 

                    await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"HEAD:{id}, Getting file head in filestream (legacy)");
                    file = _fileStreamRepo.GetFileHead(fileId);
                }

                if (file == null)
                {
                    // the file was not found in either FileStore or legacy database 
                    return NotFound();
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("0"));

                // return file info in headers


                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment) { FileName = file.FileName };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                response.Content.Headers.ContentLength = file.FileSize;

                SetHeaderContent(response, file);
                return ResponseMessage(response);
            }
            catch (FormatException ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"HEAD:{id}, bad request", ex));
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"HEAD:{id}, Exception:{ex.Message}", ex));
                return InternalServerError();
            }
        }

        /// <summary>
        /// GetFileContent
        /// </summary>
        /// <remarks>
        /// Downloads a file.
        /// </remarks>
        /// <param name="id">The GUID of the file.</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The id is missing or malformed.</response>
        /// <response code="404">Not Found. The file does not exist or has expired.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("{id}"), SessionRequired(true)]
        public async Task<IHttpActionResult> GetFileContent(string id)
        {
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, Getting file");

            Models.File file;

            try
            {
                var fileId = Models.File.ConvertToStoreId(id);

                file = await _filesRepo.GetFileHead(fileId);

                if (file == null && _fileStreamRepo.FileExists(fileId))
                {
                    // if the file is not found in the FileStore check the
                    // legacy database for the file

                    await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, Getting file from the filestream (legacy file)");
                    file = _fileStreamRepo.GetFileHead(fileId);
                }

                if (file == null)
                {
                    // the file was not found in either FileStore or legacy database 
                    return NotFound();
                }

                if (file.ExpiredTime.HasValue && file.ExpiredTime.Value <= DateTime.UtcNow)
                {
                    await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, File has expired, returning not found");
                    return NotFound();
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);

                IPushStream pushStream;
                await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, Initializing push stream");
                if (file.IsLegacyFile)
                {
                    // retrieve file content from legacy database 
                    pushStream = new FileStreamPushStream();
                    ((FileStreamPushStream)pushStream).Initialize(_fileStreamRepo, _configRepo, fileId);
                }
                else
                {
                    // retrieve file content from FileStore database 
                    // Note: In the WriteToStream method, we proceed to read the file chunks progressively from the db
                    // and flush these bits to the output stream.
                    pushStream = new SqlPushStream();
                    ((SqlPushStream)pushStream).Initialize(_filesRepo, fileId);
                }

                await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, Adding content to the response");
                //Please do not remove the redundant casting
                response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)pushStream.WriteToStream, new MediaTypeHeaderValue(file.ContentType));

                if (response.Content != null)
                {
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment)
                    {
                        FileName = file.FileName
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                    response.Content.Headers.ContentLength = file.FileSize;
                }

                SetHeaderContent(response, file);

                await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"GET:{id}, Returning file \'{file.FileName}\'");
                return ResponseMessage(response);
            }
            catch (FormatException)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, $"GET:{id}, bad request");
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"GET:{id}, Exception:{ex.Message}", ex));
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// PostFile
        /// </summary>
        /// <remarks>
        /// Uploads a file.
        /// </remarks>
        /// <param name="expired">The expiration date and time, if any.</param>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPost]
        [Route(""), SessionRequired]
        [ResponseType(typeof(UploadResult))]
        public async Task<IHttpActionResult> PostFile(DateTime? expired = null)
        {
            try
            {
                await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"POST: Initiate post");

                if (HttpContext.Current == null)
                {
                    await _log.LogError(WebApiConfig.LogSourceFiles, "POST: httpcontext.current is null");
                    return InternalServerError();
                }

                var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

                var uploadResult = await PostFileHttpContext(httpContextWrapper, expired);
                return ConstructHttpActionResult(uploadResult);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"POST: Exception:{ex.Message}", ex));
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// PutFile
        /// </summary>
        /// <remarks>
        /// Appends a chunk to an existing file.
        /// </remarks>
        /// <param name="id">The GUID of the file.</param>
        /// <response code="200">OK.</response>
        /// <response code="404">Not Found. The file does not exist or has expired.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpPut]
        [Route("{id}"), SessionRequired]
        [ResponseType(typeof(UploadResult))]
        public async Task<IHttpActionResult> PutFile(string id)
        {
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"PUT:{id}, Initiate PUT");
            try
            {
                if (HttpContext.Current == null)
                {
                    return InternalServerError();
                }

                var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

                var uploadResult = await PutFileHttpContext(id, httpContextWrapper);
                return ConstructHttpActionResult(uploadResult);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"PUT:{id}, Exception{ex.Message}", ex));
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// DeleteFile
        /// </summary>
        /// <remarks>
        /// Updates the expiration date and time of an existing file.
        /// </remarks>
        /// <param name="id">The GUID of the file.</param>
        /// <param name="expired">The expiration date and time, if any.</param>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The id is missing or malformed.</response>
        /// <response code="404">Not Found. The file does not exist or has expired.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpDelete]
        [Route("{id}"), SessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteFile(string id, DateTime? expired = null)
        {
            try
            {
                await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"DELETE:{id}, {expired}, Deleting file");
                var guid = await _filesRepo.DeleteFile(Models.File.ConvertToStoreId(id), expired);
                if (guid.HasValue)
                {
                    await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"DELETE:{id}, {expired}, Deleting file success");
                    return Ok(Models.File.ConvertFileId(guid.Value));
                }
                return NotFound();
            }
            catch (FormatException)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, $"DELETE:{id}, bad request");
                return BadRequest();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceFiles, new Exception($"DELETE:{id}, Exception:{ex.Message}", ex));
                return InternalServerError(ex);
            }
        }

        private void SetHeaderContent(HttpResponseMessage response, Models.File file)
        {
            response.Headers.Add(StoredDate, file.StoredTime.ToStringInvariant(StoredDateFormat));
            response.Headers.Add(FileSize, file.FileSize.ToStringInvariant());
            response.Headers.Add(FileChunkCount, file.ChunkCount.ToStringInvariant());
        }

        #endregion

        #region POST Logic

        internal async Task<UploadResult> PostFileHttpContext(HttpContextWrapper httpContextWrapper, DateTime? expired)
        {
            using (var stream = httpContextWrapper.Request.GetBufferlessInputStream())
            {
                var isMultipart = Request.Content.IsMimeMultipartContent();
                if (isMultipart)
                {
                    return await PostMultipartRequest(stream, expired);
                }
                return await PostNonMultipartRequest(stream, expired);
            }
        }

        private async Task<UploadResult> PostMultipartRequest(Stream stream, DateTime? expired)
        {
            UploadResult result = null;

            using (var postReader = new PostMultipartReader(stream, expired, PostCompleteFile, _log))
            {
                try
                {
                    await postReader.ReadAndExecuteRequestAsync();
                    var fileId = postReader.GetFileId();

                    result = new UploadResult
                    {
                        FileId = fileId,
                        Status = fileId.HasValue ? HttpStatusCode.Created : HttpStatusCode.BadRequest
                    };
                }
                catch (MultipartReadException)
                {
                    var guid = postReader.GetFileId();

                    if (guid != null)
                    {
                        await DeleteFile(Models.File.ConvertFileId(guid.Value));
                    }
                    result = new UploadResult
                    {
                        FileId = null,
                        Status = HttpStatusCode.BadRequest
                    };
                }
            }

            return result;
        }

        private async Task<FileChunk> PostCompleteFile(string fileName, string fileType, Stream stream, DateTime? expired)
        {
            var chunk = await PostFileHeader(fileName, fileType, expired);
            try
            {
                var fileSize = await PostFileInChunks(stream, chunk);

                await _filesRepo.UpdateFileHead(chunk.FileId, fileSize, chunk.ChunkNum - 1);
            }
            catch
            {
                // Deleting file since there was an exception in uploading the chunks or updating file head, meaning the file is only partially uploaded.                
                await DeleteFile(chunk.FileId.ToString());
                throw;
            }

            return chunk;
        }

        private async Task<UploadResult> PostNonMultipartRequest(Stream stream, DateTime? expired)
        {
            string decodedFileName = "";
            if (Request.Content.Headers.ContentDisposition != null)
                decodedFileName = HttpUtility.UrlDecode(Request.Content.Headers.ContentDisposition.FileName);
            if (string.IsNullOrEmpty(decodedFileName) || 
                //string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition?.FileName) ||
                string.IsNullOrWhiteSpace(Request.Content.Headers.ContentType?.MediaType))
            {
                return new UploadResult
                {
                    FileId = null,
                    Status = HttpStatusCode.BadRequest
                };
            }
            // Grabs all available information from the header
            var fileName = decodedFileName.Replace("\"", string.Empty).Replace("%20", " ");
            var fileMediaType = Request.Content.Headers.ContentType.MediaType;
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"POST: Posting non-multi-part file {fileName}");
            var chunk = await PostCompleteFile(fileName, fileMediaType, stream, expired);
            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"POST: Chunks posted {chunk.ChunkNum - 0}");

            return new UploadResult
            {
                FileId = chunk.FileId,
                Status = HttpStatusCode.Created
            };
        }

        /// <summary>
        /// Posts the file from the stream in multiple chunks and returns the file size.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="chunk"></param>
        /// <returns>The total size of the file that was inserted into the db.</returns>
        private async Task<long> PostFileInChunks(Stream stream, FileChunk chunk)
        {
            long fileSize = 0;
            chunk.ChunkSize = _configRepo.FileChunkSize;
            var buffer = new byte[_configRepo.FileChunkSize];
            for (var readCounter = stream.Read(buffer, 0, _configRepo.FileChunkSize);
                          readCounter > 0;
                 readCounter = stream.Read(buffer, 0, _configRepo.FileChunkSize))
            {
                chunk.ChunkSize = readCounter;
                chunk.ChunkContent = buffer.Take(readCounter).ToArray();
                chunk.ChunkNum = await _filesRepo.PostFileChunk(chunk);
                fileSize += chunk.ChunkSize;
            }
            return fileSize;
        }

        /// <summary>
        /// Posts the initial file header info and returns the first chunk with the FileId (guid) created in the database.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mediaType"></param>
        /// <param name="expired"></param>
        /// <returns></returns>
        private async Task<FileChunk> PostFileHeader(string fileName, string mediaType, DateTime? expired)
        {
            //we can access the filename from the part
            var file = new Models.File
            {
                StoredTime = DateTime.UtcNow, // use UTC time to store data
                FileName = fileName,
                FileType = mediaType,
                ExpiredTime = expired
            };

            var fileId = await _filesRepo.PostFileHead(file);
            var chunk = new FileChunk
            {
                FileId = fileId,
                ChunkNum = 1
            };

            return chunk;
        }
        #endregion

        #region PUT Logic

        internal async Task<UploadResult> PutFileHttpContext(string id, HttpContextWrapper httpContextWrapper)
        {
            var fileId = Models.File.ConvertToStoreId(id);
            var fileHead = await _filesRepo.GetFileHead(fileId);
            if (fileHead == null)
            {
                return new UploadResult
                {
                    FileId = null,
                    Status = HttpStatusCode.NotFound
                };
            }
            int startingChunkNumber = fileHead.ChunkCount + 1;
            var chunk = new FileChunk()
            {
                ChunkNum = startingChunkNumber,
                FileId = fileHead.FileId
            };

            long fileSize;
            try
            {
                using (var stream = httpContextWrapper.Request.GetBufferlessInputStream())
                {
                    var isMultipart = Request.Content.IsMimeMultipartContent();
                    if (isMultipart)
                    {
                        fileSize = await PutFileMultipart(stream, chunk);
                    }
                    else
                    {
                        fileSize = await PostFileInChunks(stream, chunk);
                    }
                }
                await _filesRepo.UpdateFileHead(chunk.FileId, fileHead.FileSize + fileSize, chunk.ChunkNum - 1);
            }
            catch
            {
                // Delete all chunks after the starting chunk of this PUT.
                await DeleteFileChunks(chunk.FileId, startingChunkNumber);
                throw;
            }

            await _log.LogVerbose(WebApiConfig.LogSourceFiles, $"PUT:{id}, Chunks were added in PUT. Total chunks in file:{chunk.ChunkNum - 1}");
            return new UploadResult
            {
                FileId = chunk.FileId,
                Status = HttpStatusCode.OK
            };
        }

        internal async Task<long> PutFileMultipart(Stream stream, FileChunk chunk)
        {
            using (var putFileMultipart = new PutMultipartReader(stream, chunk, PostFileInChunks, _log))
            {
                await putFileMultipart.ReadAndExecuteRequestAsync();
                var fileSize = putFileMultipart.GetFileSize();
                if (fileSize.HasValue)
                {
                    return fileSize.Value;
                }
                throw new Exception("File size does not have a value after executing the PUT");
            }
        }

        private async Task<int> DeleteFileChunks(Guid guid, int startingChunkNumber)
        {
            int rowsAffected = 0;
            int chunkNumber = startingChunkNumber;
            while (await _filesRepo.DeleteFileChunk(guid, chunkNumber++) > 0)
            {
                rowsAffected++;
            }
            return rowsAffected;
        }

        #endregion

        #region Http Result

        private IHttpActionResult ConstructHttpActionResult(UploadResult uploadResult)
        {

            if (uploadResult.Status == HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            if (uploadResult.Status == HttpStatusCode.BadRequest || !uploadResult.FileId.HasValue)
            {
                return BadRequest();
            }
            if (uploadResult.Status == HttpStatusCode.Created)
            {
                var uri = new Uri(I18NHelper.FormatInvariant("{0}/{1}", Request.RequestUri.LocalPath, uploadResult.FileId.Value), UriKind.Relative);
                return Created(uri, uploadResult.FileId.Value);
            }
            if (uploadResult.Status == HttpStatusCode.OK)
            {
                return Ok(uploadResult.FileId.Value);
            }
            return new NegotiatedContentResult<Guid>(uploadResult.Status, uploadResult.FileId.Value, this);
        }

        #endregion

    }

}
