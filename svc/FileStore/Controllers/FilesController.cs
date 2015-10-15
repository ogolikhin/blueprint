using System;
using System.Linq;
using System.Threading.Tasks;
using FileStore.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;

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

        private const string CacheControl = "Cache-Control";
        private const string Pragma = "Pragma";
        private const string StoredDate = "Stored-Date";
        private const string FileSize = "File-Size";
        private const string Attachment = "attachment";
        private const string NoCache = "no-cache";
        private const string NoStore = "no-store";
        private const string MustRevalidate = "must-revalidate";

        public FilesController() : this(new SqlFilesRepository(), new FileStreamRepository(), new FileMapperRepository())
        {
        }

        internal FilesController(IFilesRepository fr, IFileStreamRepository fsr, IFileMapperRepository fmr)
        {
            _filesRepo = fr;
            _fileStreamRepo = fsr;
            _fileMapperRepo = fmr;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> PostFile()
        {
            try
            {
                var isMultipart = Request.Content.IsMimeMultipartContent();
                Models.File file;
                if (isMultipart)
                {
                    var multipartMemoryStreamProvider = await Request.Content.ReadAsMultipartAsync();
                    if (multipartMemoryStreamProvider.Contents.Count > 1)
                    {
                        return BadRequest();
                    }
                    var httpContent = multipartMemoryStreamProvider.Contents.First();
                    file = await GetFileInfo(httpContent);
                }
                else
                {
                    //Temporarily allow only multipart uploads
                    if (Request.Content.Headers.ContentDisposition == null ||
                       string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition.FileName) ||
                       Request.Content.Headers.ContentType == null ||
                       string.IsNullOrWhiteSpace(Request.Content.Headers.ContentType.MediaType))
                    {
                        return BadRequest();
                    }
                    file = await GetFileInfo(Request.Content);
                }

                var postFileResult = await _filesRepo.PostFile(file);
                file.FileId = postFileResult.Value;
                return Ok(Models.File.ConvertFileId(file.FileId));
            }
            catch
            {
                return InternalServerError();
            }
        }

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
                
                var isFileStoreGuid = false;

                Guid guid = Guid.Empty;

                try
                {
                    guid = Models.File.ConvertToFileStoreId(id);
                    isFileStoreGuid = true;
                }
                catch (FormatException)
                {
                    guid = Models.File.ConvertToBlueprintStoreId(id);
                    isFileStoreGuid = false;
                }

                if (guid == Guid.Empty)
                {
                    return BadRequest();
                }

                if (isHead)
                {
                    file = await _filesRepo.HeadFile(guid) ?? _fileStreamRepo.HeadFile(guid);
                }
                else
                {
                    file = await _filesRepo.GetFile(guid) ?? _fileStreamRepo.GetFile(guid);
                }

                if (file == null || (!isFileStoreGuid && file.FileName == ""))
                {
                    return NotFound();
                }

                var mappedContentType = new FileMapperRepository().GetMappedOutputContentType(file.FileType);

                //var originalRequestContentMediaType = GetRequestContentMediaType();
                //if (!string.IsNullOrWhiteSpace(originalRequestContentMediaType) && !string.Equals(originalRequestContentMediaType, mappedContentType, StringComparison.InvariantCultureIgnoreCase))
                //{
                //    return BadRequest();
                //}

                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Content = isHead ? new ByteArrayContent(Encoding.UTF8.GetBytes("")) : new ByteArrayContent(file.FileContent);

                response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
                response.Headers.Add(Pragma, NoCache); // HTTP 1.0.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment) { FileName = file.FileName };
                response.Content.Headers.ContentType = isFileStoreGuid ? new MediaTypeHeaderValue(file.FileType) : 
                    !string.IsNullOrWhiteSpace(mappedContentType) ? new MediaTypeHeaderValue(mappedContentType) : null;
                response.Content.Headers.ContentLength = file.FileSize;
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
        public Task<IHttpActionResult> DeleteFile(string id)
        {
            throw new NotSupportedException();
        }


        #region Private Methods

        private async Task<Models.File> GetFileInfo(HttpContent httpContent)
        {
            using (var stream = await httpContent.ReadAsStreamAsync())
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    var fileArray = memoryStream.ToArray();
                    return new Models.File()
                    {
                        StoredTime = DateTime.UtcNow, // use UTC time to store data
                        FileName = httpContent.Headers.ContentDisposition.FileName.Replace("\"", string.Empty).Replace("%20", " "),
                        FileType = httpContent.Headers.ContentType.MediaType,
                        FileSize = httpContent.Headers.ContentLength.GetValueOrDefault(),
                        FileContent = fileArray
                    };
                }
            }
        }

        private string GetRequestContentMediaType()
        {
            string contentType = null;

            if (Request != null &&
                Request.Content != null &&
                Request.Content.Headers != null &&
                Request.Content.Headers.ContentType != null)
            {
                contentType = Request.Content.Headers.ContentType.MediaType;
            }
            return contentType;
        }

        #endregion
    }
}
