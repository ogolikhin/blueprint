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
using HttpMultipartParser;

namespace FileStore.Controllers
{
	[RoutePrefix("files")]
	public class FilesController : ApiController
	{
		private readonly IFilesRepository _fr;

		public FilesController() : this(new SqlFilesRepository())
		{
		}

		internal FilesController(IFilesRepository fr)
		{
			_fr = fr;
		}

		[HttpPost]
		[Route("")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile()
		{
			var parser = new MultipartFormDataParser(await Request.Content.ReadAsStreamAsync(), Encoding.UTF8);
			var upload = parser.Files.First();
			var stream = new MemoryStream();
			upload.Data.CopyTo(stream);
			var file = new Models.File()
			{
				StoredTime = DateTime.UtcNow, // use UTC time to store data
				FileName = upload.FileName.Replace("%20", " "),
				FileType = upload.ContentType,
				FileSize = upload.Data.Length,
				FileContent = stream.ToArray()
			};
			try
			{
				await _fr.PostFile(file);
			}
			catch
			{
				return InternalServerError();
			}
			return Ok(Models.File.ConvertFileId(file.FileId));
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
