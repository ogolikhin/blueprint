using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore.Models;

namespace FileStore.Controllers
{
	[RoutePrefix("api/books")]
	public class FilesController : ApiController
	{
		private readonly FileStoreContext _db = new FileStoreContext();

		[HttpHead]
		[Route("files/{id}")]
		[ResponseType(typeof(FileDetail))]
		public async Task<IHttpActionResult> GetFileDetail(string id)
		{
			Guid guid;
			if (!Guid.TryParseExact(id, "N", out guid))
			{
				return BadRequest();
			}
			var fd = await _db.FileDetails.FindAsync(guid);
			if (fd == null)
			{
				// TODO: CHECK FILESTREAM
				return NotFound();
			}
			return Ok(fd);
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
			var file = await _db.Files.FindAsync(guid);
			if (file == null)
			{
				// TODO: CHECK FILESTREAM
				return NotFound();
			}
			var response = Request.CreateResponse(HttpStatusCode.OK);
			response.Content = new ByteArrayContent(file.FileContent);
			response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
			response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
			response.Headers.Add("Content-Disposition", "filename=\"" + file.FileName + "\"");
			response.Headers.Add("Content-Type", file.FileType);
			return ResponseMessage(response);
		}

		[HttpPost]
		[Route("files")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile()
		{
			var file = new File();
			// TODO: POPULATE FILE with info from headers
			_db.Files.Add(file);
			try
			{
				await _db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				return Conflict();
			}
			return Ok(file.FileId);
		}

		[HttpDelete]
		[Route("files/{id}")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> DeleteFile(string id)
		{
			Guid guid;
			if (!Guid.TryParseExact(id, "N", out guid))
			{
				return BadRequest();
			}
			var file = await _db.Files.FindAsync(guid);
			if (file == null)
			{
				// TODO: CHECK FILESTREAM and DELETE FILESTREAM
				return NotFound();
			}
			_db.Files.Remove(file);
			await _db.SaveChangesAsync();
			return Ok(file.FileId);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}