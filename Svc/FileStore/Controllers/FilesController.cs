using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DataProtection.Repositories;
using Microsoft.AspNet.Mvc;
using FileStore.Repositories;
using System.Net;
using Microsoft.AspNet.Http;
using HttpMultipartParser;
using System.Text;
using System.IO;

namespace FileStore.Controllers
{
	[Route("files")]
	public class FilesController : Controller
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
		public async Task<IActionResult> PostFile()
		{
			//TODO: replace MultipartFormDataParser with one used in Blueprint
			var pf = (new MultipartFormDataParser(Request.Body, Encoding.UTF8)).Files.First();

			var file = new Models.File()
			{
				StoredTime = DateTime.UtcNow, // use UTC time to store data
				FileName = pf.FileName.Replace("%20", " "),
				FileType = pf.ContentType
			};
			// TODO: FORWARD STREAM FROM REQUEST TO DB - file.FileContent = ReadFully(data);
			var stream = pf.Data;
			stream.Position = 0;
			byte[] buffer = new byte[16 * 1024];
			using (var ms = new System.IO.MemoryStream())
			{
				int read;
				while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
				{
					ms.Write(buffer, 0, read);
				}
				file.FileContent = ms.ToArray();
			}
			try
			{
				await _fr.PostFile(file);
			}
			catch
			{
				return new HttpStatusCodeResult(Response.StatusCode = 500);
			}
			Response.StatusCode = 200;
			return Content(Models.File.ConvertFileId(file.FileId));
		}

		[AcceptVerbs("HEAD")]
		[Route("files/{id}")]
		public async Task<IActionResult> HeadFile(string id)
		{
			try
			{
				var file = await _fr.HeadFile(Models.File.ConvertFileId(id));
				if (file == null)
				{
					// TODO: CHECK FILESTREAM
					return HttpNotFound();
				}
				Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
				Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
				Response.Headers["Unique-Identifier"] = file.FileId.ToString("N");
				Response.Headers["Stored-Date"] = file.StoredTime.ToString("o");
				Response.Headers["Content-Disposition"] = "attachment;filename=" + file.FileName;
				Response.ContentType = file.FileType;
				Response.StatusCode = 204;
				return new HttpStatusCodeResult(Response.StatusCode);
			}
			catch (FormatException)
			{
				return HttpBadRequest();
			}
      }

		[HttpGet("{id}")]
		public async Task<IActionResult> GetFile(string id)
		{
			try
			{
				var file = await _fr.GetFile(Models.File.ConvertFileId(id));
				if (file == null)
				{
					// TODO: CHECK FILESTREAM
					return HttpNotFound();
				}
				Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate"; // HTTP 1.1.
				Response.Headers["Pragma"] = "no-cache"; // HTTP 1.0.
				Response.Headers["Unique-Identifier"] = file.FileId.ToString("N");
				Response.Headers["Stored-Date"] = file.StoredTime.ToString("o");
				Response.Headers["Content-Disposition"] = "attachment;filename=" + file.FileName;
				Response.ContentType = file.FileType;
				Response.StatusCode = 200;
				// TODO: FORWARD STREAM FROM DB TO RESPONSE - Response.Body = file.FileContent;
				return File(file.FileContent, file.FileType);
			}
			catch (FormatException)
			{
				return HttpBadRequest();
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteFile(string id)
		{
			try
			{
				var guid = await _fr.DeleteFile(Models.File.ConvertFileId(id));
            if (guid != null)
				{
					Response.StatusCode = 200;
					return Content(Models.File.ConvertFileId(guid.Value));
				}
				else
				{
					// TODO: CHECK FILESTREAM
					return HttpNotFound();
				}
			}
			catch (FormatException)
			{
				return HttpBadRequest();
			}
		}
	}
}
