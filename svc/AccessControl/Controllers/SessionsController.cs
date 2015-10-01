using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessCotrol.Repositories;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web;

namespace AccessCotrol.Controllers
{
    [RoutePrefix("files")]
    public class SessionsController : ApiController
    {
        private readonly IFilesRepository _fileRepo;

        public SessionsController() : this(new SqlFilesRepository())
        {
        }

        internal SessionsController(IFilesRepository fr)
        {
            _fileRepo = fr;
        }

        [HttpPost]
        [Route("")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> PostFile()
        {
            try
            {
                var isMultipart = Request.Content.IsMimeMultipartContent();
                Models.File file = null;
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

                var postFileResult = await _fileRepo.PostFile(file);
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
                Models.File file = null;
                bool isHead = Request.Method == HttpMethod.Head;
                if (isHead)
                {
                    file = await _fileRepo.HeadFile(Models.File.ConvertFileId(id));
                }
                else
                {
                    file = await _fileRepo.GetFile(Models.File.ConvertFileId(id));
                }
                if (file == null)
                {
                    // TODO: CHECK FILESTREAM
                    return NotFound();
                }
                var response = Request.CreateResponse(HttpStatusCode.OK);
                if (isHead)
                {
                    response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(""));
                }
                else
                {
                    response.Content = new ByteArrayContent(file.FileContent);
                }
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = file.FileName };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.FileType);
                response.Content.Headers.ContentLength = file.FileSize;
                response.Headers.Add("Stored-Date", file.StoredTime.ToString("o"));
                response.Headers.Add("File-Size", file.FileSize.ToString());
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
                var guid = await _fileRepo.DeleteFile(Models.File.ConvertFileId(id));
                if (guid.HasValue)
                {
                    return Ok(Models.File.ConvertFileId(guid.Value));
                }
                else
                {
                    // TODO: CHECK FILESTREAM
                    return NotFound();
                }
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

        #endregion
    }
}
