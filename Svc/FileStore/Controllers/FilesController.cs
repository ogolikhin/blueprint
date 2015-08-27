using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FileStore.Models;

namespace FileStore.Controllers
{
	[RoutePrefix("api/books")]
	public class FilesController : ApiController
	{
		private FileStoreContext db = new FileStoreContext();

		// GET: api/Files
		public IQueryable<FileRecord> GetFileRecords()
		{
			return db.FileRecords;
		}

		// GET: api/Files/5
		[ResponseType(typeof(FileRecord))]
		public async Task<IHttpActionResult> GetFileRecord(string id)
		{
			FileRecord fileRecord = await db.FileRecords.FindAsync(id);
			if (fileRecord == null)
			{
				return NotFound();
			}

			return Ok(fileRecord);
		}

		// PUT: api/Files/5
		[ResponseType(typeof(void))]
		public async Task<IHttpActionResult> PutFileRecord(string id, FileRecord fileRecord)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			if (id != fileRecord.FileId)
			{
				return BadRequest();
			}

			db.Entry(fileRecord).State = EntityState.Modified;

			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!FileRecordExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}

			return StatusCode(HttpStatusCode.NoContent);
		}

		// POST: api/Files
		[ResponseType(typeof(FileRecord))]
		public async Task<IHttpActionResult> PostFileRecord(FileRecord fileRecord)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			db.FileRecords.Add(fileRecord);

			try
			{
				await db.SaveChangesAsync();
			}
			catch (DbUpdateException)
			{
				if (FileRecordExists(fileRecord.FileId))
				{
					return Conflict();
				}
				else
				{
					throw;
				}
			}

			return CreatedAtRoute("DefaultApi", new { id = fileRecord.FileId }, fileRecord);
		}

		// DELETE: api/Files/5
		[ResponseType(typeof(FileRecord))]
		public async Task<IHttpActionResult> DeleteFileRecord(string id)
		{
			FileRecord fileRecord = await db.FileRecords.FindAsync(id);
			if (fileRecord == null)
			{
				return NotFound();
			}

			db.FileRecords.Remove(fileRecord);
			await db.SaveChangesAsync();

			return Ok(fileRecord);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}

		private bool FileRecordExists(string id)
		{
			return db.FileRecords.Count(e => e.FileId == id) > 0;
		}
	}
}