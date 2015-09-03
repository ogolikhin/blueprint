using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore.Models;
using System.Web;
using HttpMultipartParser;
using System.Net.Http.Headers;
using FileStore.Repo;
using System.Net.Http.Formatting;

namespace FileStore.Controllers
{
	[RoutePrefix("")]
	public sealed class FilesController : ApiController
	{
        private IRepo _repo;

        public FilesController()
        {
            _repo = new SqlRepo();
        }

        [HttpHead]
		[Route("files/{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFileDetail(string id)
		{
			Guid guid;
			if (!Guid.TryParseExact(id, "N", out guid))
			{
				return BadRequest();
			}
            File fileInfo = await _repo.GetFileInfo(guid);
            if (fileInfo == null)
			{
				// TODO: CHECK FILESTREAM
				return NotFound();
			}

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
            response.Headers.Add("File-Name", fileInfo.FileName);
            response.Headers.Add("File-Type", fileInfo.FileType);
            response.Headers.Add("Stored-Date", fileInfo.StoredTime.ToString("o"));
            return ResponseMessage(response);
        }

        [HttpGet]
		[Route("files/{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFile(string id)
		{
			Guid guid;
			if (!Guid.TryParseExact(id, "N", out guid))
			{
				return BadRequest();
			}

            File file = await _repo.GetFile(guid);
            
            if (file == null)
			{
				// TODO: CHECK FILESTREAM
				return NotFound();
			}

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ByteArrayContent(file.FileContent);
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = file.FileName };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.FileType);
            response.Headers.Add("Stored-Date", file.StoredTime.ToString("o"));
            return ResponseMessage(response);
		}

		[HttpPost]
		[Route("files")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile()
		{
            var stream = HttpContext.Current.Request.InputStream;
            stream.Position = 0;

            //TODO: replace MultipartFormDataParser with one used in Blueprint
            var parser = new MultipartFormDataParser(stream, Encoding.UTF8);
            var file = parser.Files.First();

            System.IO.Stream data = file.Data;

            var fileData = new File();
            // Unescape space in file name
            fileData.FileId = Guid.NewGuid();
            fileData.FileName = file.FileName.Replace("%20", " ");
            fileData.FileContent = ReadFully(data);
            fileData.FileType = file.ContentType;
            fileData.StoredTime = DateTime.Now;

            // TODO: POPULATE FILE with info from headers
			try
			{
                if (!await _repo.AddFile(fileData))
                {
                    return Conflict();
                }
            }
            catch 
			{
				return Conflict();
			}
			return Ok(fileData.FileId);
		}

		[HttpDelete]
		[Route("files/{id}")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> DeleteFile(string id)
		{
			Guid fileId;
			if (!Guid.TryParseExact(id, "N", out fileId))
			{
				return BadRequest();
			}
            File file = await _repo.GetFileInfo(fileId);
            if (file == null)
            {
                // TODO: CHECK FILESTREAM
                return NotFound();
            }
            else
            {
                if (!await _repo.DeleteFile(fileId))
                {
                    return Conflict();
                }
            }
			return Ok(file.FileId);
		}

        private static byte[] ReadFully(System.IO.Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new System.IO.MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}