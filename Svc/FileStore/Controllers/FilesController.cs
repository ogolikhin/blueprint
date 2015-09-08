using System;
using System.Collections.Generic;
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
using System.Web;

namespace FileStore.Controllers
{
	[RoutePrefix("files")]
	public class FilesController : ApiController
	{
		private readonly IFilesRepository _fr;
        private Stream _inputStream;

        public FilesController() : this(new SqlFilesRepository(), null)
		{
		}

		internal FilesController(IFilesRepository fr, Stream inputStream)
		{
			_fr = fr;
            _inputStream = inputStream;
        }

        [HttpPost]
		[Route("")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile()
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
                if (Request.Content.Headers.ContentDisposition == null || 
                    string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition.FileName))
                {
                    return BadRequest();
                }
                file = await GetFileInfo(Request.Content);
            }
            
			try
			{
				await _fr.PostFile(file);
			}
			catch (Exception ex)
			{
                System.Diagnostics.Trace.Write(ex);
				return InternalServerError();
			}
			return Ok(Models.File.ConvertFileId(file.FileId));
		}

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

		[HttpHead]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> HeadFile(string id)
		{
			try
			{
				var file = await _fr.HeadFile(Models.File.ConvertFileId(id));
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
			catch (FormatException)
			{
				return BadRequest();
			}
			catch
			{
				return InternalServerError();
			}
		}

		[HttpGet]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFile(string id)
		{
			try
			{
				var file = await _fr.GetFile(Models.File.ConvertFileId(id));
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
				var guid = await _fr.DeleteFile(Models.File.ConvertFileId(id));
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
	}
}
