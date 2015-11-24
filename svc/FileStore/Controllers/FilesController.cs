using System;
using System.Linq;
using System.Threading.Tasks;
using FileStore.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore.Helpers;
using FileStore.Models;

namespace FileStore.Controllers
{
    [RoutePrefix("files")]
    public class FilesController : ApiController
    {

        //remove unnecessary headers from web api
        //http://www.4guysfromrolla.com/articles/120209-1.aspx

        private readonly IFilesRepository _filesRepo;
        private readonly IFileStreamRepository _fileStreamRepo;
        private readonly IFileMapperRepository _fileMapperRepo;
        private readonly IConfigRepository _configRepo;

        private const string CacheControl = "Cache-Control";
        private const string Pragma = "Pragma";
        private const string StoredDate = "Stored-Date";
        private const string FileSize = "File-Size";
        private const string Attachment = "attachment";
        private const string NoCache = "no-cache";
        private const string NoStore = "no-store";
        private const string MustRevalidate = "must-revalidate";

        public FilesController() : this(new SqlFilesRepository(), new FileStreamRepository(), new FileMapperRepository(), ConfigRepository.Instance)
        {
        }

        internal FilesController(IFilesRepository fr, IFileStreamRepository fsr, IFileMapperRepository fmr, IConfigRepository cr)
        {
            _filesRepo = fr;
            _fileStreamRepo = fsr;
            _fileMapperRepo = fmr;
            _configRepo = cr;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> PostFile()
        {
            if(HttpContext.Current == null)
                return InternalServerError();
            var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
            return await PostFileHttpContext(httpContextWrapper);
        }

        public async Task<IHttpActionResult> PostFileHttpContext(HttpContextWrapper httpContextWrapper)
        {
            try
            {
                using (var stream = httpContextWrapper.Request.GetBufferlessInputStream())
                {
                    var isMultipart = Request.Content.IsMimeMultipartContent();
                    if (isMultipart)
                    {
                        return await PostMultipartRequest(stream);
                    }
                    else
                    {
                        return await PostNonMultipartRequest(stream);
                    }
                }
            }
            catch
            {
                return InternalServerError();
            }
        }

        private async Task<IHttpActionResult> PostMultipartRequest(Stream stream)
        {
            var mpp = new MultipartPartParser(stream);
            if (mpp.IsEndPart)
            {
                return BadRequest();
            }
            while (!mpp.IsEndPart && !string.IsNullOrWhiteSpace(mpp.Filename))
            {
                // Gets current part's header information
                var fileName = mpp.Filename.Replace("\"", string.Empty).Replace("%20", " ");
                var fileType = mpp.ContentType;

                var chunk = await PostCompleteFile(fileName, fileType, mpp);

                //move the stream foward until we get to the next part
                mpp = mpp.ReadUntilNextPart();
                if (mpp != null)
                {
                    // Right now we are only supporting uploading the first part of multipart. Can easily change it to upload more than one.
                    await _filesRepo.DeleteFile(chunk.FileId);
                    return BadRequest();
                }
                return Ok(Models.File.ConvertFileId(chunk.FileId));
            }
            return BadRequest();
        }

        private async Task<FileChunk> PostCompleteFile(string fileName, string fileType, Stream stream)
        {
            var chunk = await PostFileHeader(fileName, fileType);

            var fileSize = await PostFileInChunks(stream, chunk);

            _filesRepo.UpdateFileHead(chunk.FileId, fileSize, chunk.ChunkNum - 1);

            return chunk;
        }

        private async Task<IHttpActionResult> PostNonMultipartRequest(Stream stream)
        {
            if (string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition?.FileName) ||
                string.IsNullOrWhiteSpace(Request.Content.Headers.ContentType?.MediaType))
            {
                return BadRequest();
            }
            // Grabs all available information from the header
            var fileName = Request.Content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty).Replace("%20", " ");
            var fileMediaType = Request.Content.Headers.ContentType.MediaType;

            var chunk = await PostCompleteFile(fileName, fileMediaType, stream);

            return Ok(Models.File.ConvertFileId(chunk.FileId));
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
            for (var readCounter = await stream.ReadAsync(buffer, 0, chunk.ChunkSize);
                    readCounter > 0;
                    readCounter = await stream.ReadAsync(buffer, 0, chunk.ChunkSize))
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
        /// <returns></returns>
        private async Task<Models.FileChunk> PostFileHeader(string fileName, string mediaType)
        {
            //we can access the filename from the part
            var file = new Models.File
            {
                StoredTime = DateTime.UtcNow, // use UTC time to store data
                FileName = fileName,
                FileType = mediaType
            };
            var fileId = await _filesRepo.PostFileHead(file);
            var chunk = new Models.FileChunk
            {
                FileId = fileId,
                ChunkNum = 1
            };

            return chunk;
        }

        #endregion Post file methods
        [HttpGet]
        [HttpHead]
        [Route("{id}")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetFile(string id)
        {
            try
            {
                Models.File file;
                bool isHead = Request.Method == HttpMethod.Head;

                var guid = Models.File.ConvertToStoreId(id);

                var isFileStoreGuid = true;
                if (isHead)
                {
                    file = await _filesRepo.GetFileHead(guid);
                    if (file == null)
                    {
                        file = _fileStreamRepo.HeadFile(guid);
                        isFileStoreGuid = false;
                    }
                }
                else
                {
                    file = await _filesRepo.GetFileHead(guid);
                    if (file == null)
                    {
                        file = _fileStreamRepo.GetFile(guid);
                        isFileStoreGuid = false;
                    }
                }

                if (file == null || (!isFileStoreGuid && string.IsNullOrEmpty(file.FileName)))
                {
                    return NotFound();
                }

                var mappedContentType = isFileStoreGuid ? file.FileType : _fileMapperRepo.GetMappedOutputContentType(file.FileType);
                if (string.IsNullOrWhiteSpace(mappedContentType))
                {
                    mappedContentType = FileMapperRepository.DefaultMediaType;
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);
                HttpContent responseContent = null;
                if (isHead)
                {
                    responseContent = new ByteArrayContent(Encoding.UTF8.GetBytes(""));
                }
                else
                {
                    if (isFileStoreGuid)
                    {
                        // TODO: fix
                        //responseContent = new ByteArrayContent(file.FileContent);
                    }
                    else
                    {
                        //responseContent = new StreamContent(file.FileStream, 1048576);
                    }
                }

                response.Content = responseContent;

                response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
                response.Headers.Add(Pragma, NoCache); // HTTP 1.0.
                if (response.Content != null)
                {
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment)
                    {
                        FileName = file.FileName
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue(mappedContentType);
                    response.Content.Headers.ContentLength = file.FileSize;
                }
                response.Headers.Add(StoredDate, file.StoredTime.ToString("o"));
                response.Headers.Add(FileSize, file.FileSize.ToString());

                return ResponseMessage(response);
            }
            catch (FormatException)
            {
                return BadRequest();
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpDelete]
        [Route("{id}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> DeleteFile(string id)
        {
            try
            {
                var guid = await _filesRepo.DeleteFile(Models.File.ConvertToStoreId(id));
                if (guid.HasValue)
                {
                    return Ok(Models.File.ConvertFileId(guid.Value));
                }
                return NotFound();
            }
            catch (FormatException)
            {
                return BadRequest();
            }
            catch
            {
                return InternalServerError();
            }
        }
    }
}
