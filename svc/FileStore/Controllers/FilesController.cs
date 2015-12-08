using System;
using System.Linq;
using System.Threading.Tasks;
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
using FileStore.Repositories;

namespace FileStore.Controllers
{
	[RoutePrefix("files")]
	public class FilesController : ApiController
	{

		//remove unnecessary headers from web api
		//http://www.4guysfromrolla.com/articles/120209-1.aspx

		private readonly IFilesRepository _filesRepo;
		private readonly IFileStreamRepository _fileStreamRepo;
		private readonly IConfigRepository _configRepo;

		private const string CacheControl = "Cache-Control";
		private const string Pragma = "Pragma";
		private const string StoredDate = "Stored-Date";
		private const string FileSize = "File-Size";
		private const string FileChunkCount = "File-Chunk-Count";
		private const string Attachment = "attachment";
		private const string NoCache = "no-cache";
		private const string NoStore = "no-store";
		private const string MustRevalidate = "must-revalidate";
		private const string StoredDateFormat = "o";

		public FilesController() : this(new SqlFilesRepository(), new FileStreamRepository(), ConfigRepository.Instance)
		{
		}

		internal FilesController(IFilesRepository fr, IFileStreamRepository fsr, IConfigRepository cr)
		{
			_filesRepo = fr;
			_fileStreamRepo = fsr;
			_configRepo = cr;
		}
        
        #region Service Methods
        [HttpHead]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFileHead(string id)
		{
			Models.File file;

			try
			{
				var fileId = Models.File.ConvertToStoreId(id);

				file = await _filesRepo.GetFileHead(fileId);

				if (file == null && _fileStreamRepo.FileExists(fileId))
				{
					// if the file is not found in the FileStore check the
					// legacy database for the file 

					file = _fileStreamRepo.GetFileHead(fileId);
				}

				if (file == null)
				{
					// the file was not found in either FileStore or legacy database 
					return NotFound();
				}

				var response = Request.CreateResponse(HttpStatusCode.OK);

				response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(""));

				// return file info in headers

				response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
				response.Headers.Add(Pragma, NoCache); // HTTP 1.0.
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment) { FileName = file.FileName };
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
				response.Content.Headers.ContentLength = 0; // there is no content
				response.Headers.Add(StoredDate, file.StoredTime.ToString(StoredDateFormat));
				response.Headers.Add(FileSize, file.FileSize.ToString());
				response.Headers.Add(FileChunkCount, file.ChunkCount.ToString());

				return ResponseMessage(response);
			}
			catch (FormatException)
			{
				return BadRequest();
			}
			catch(Exception ex)
			{
				return InternalServerError(ex);
			}
		}


		[HttpGet]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFileContent(string id)
		{
			Models.File file;

			try
			{
				var fileId = Models.File.ConvertToStoreId(id);

				file = await _filesRepo.GetFileHead(fileId);

				if (file == null && _fileStreamRepo.FileExists(fileId))
				{
					// if the file is not found in the FileStore check the
					// legacy database for the file

					file = _fileStreamRepo.GetFileHead(fileId);
				}

				if (file == null)
				{
					// the file was not found in either FileStore or legacy database 
					return NotFound();
				}

                if (file.ExpiredTime.HasValue && file.ExpiredTime.Value.ToUniversalTime() <= DateTime.UtcNow)
                {
                    return NotFound();
                }

				var response = Request.CreateResponse(HttpStatusCode.OK);

			    IPushStream pushStream;
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

                //Please do not remove the redundant casting
                response.Content = new PushStreamContent((Func<Stream, HttpContent, TransportContext, Task>)pushStream.WriteToStream, new MediaTypeHeaderValue(file.ContentType));

                response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
				response.Headers.Add(Pragma, NoCache); // HTTP 1.0.

				if (response.Content != null)
				{
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment)
					{
						FileName = file.FileName
					};
					response.Content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
					response.Content.Headers.ContentLength = file.FileSize;
				}
				response.Headers.Add(StoredDate, file.StoredTime.ToString(StoredDateFormat));
				response.Headers.Add(FileSize, file.FileSize.ToString());

				return ResponseMessage(response);
			}
			catch (FormatException)
			{
				return BadRequest();
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

        [HttpPost]
		[Route("")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile(DateTime? expired = null)
		{
		    if (expired.HasValue && expired.Value < DateTime.UtcNow)
		    {
		        expired = DateTime.UtcNow;
		    }
			if (HttpContext.Current == null)
			{
				return InternalServerError();
			}
			var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);
			return await PostFileHttpContext(httpContextWrapper, expired);
		}

        [HttpPut]
        [Route("{id}")]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> PutFile(string id)
        {
            try
            {
                if (HttpContext.Current == null)
                    return InternalServerError();
                var httpContextWrapper = new HttpContextWrapper(HttpContext.Current);

                return await PutFileHttpContext(id, httpContextWrapper);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete]
		[Route("{id}")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> DeleteFile(string id, DateTime? expired = null)
		{
		    var expirationTime = expired.HasValue && expired.Value > DateTime.UtcNow ? expired.Value : DateTime.UtcNow;
			try
			{
				var guid = await _filesRepo.DeleteFile(Models.File.ConvertToStoreId(id), expirationTime);
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
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

        #region POST Logic

        internal async Task<IHttpActionResult> PostFileHttpContext(HttpContextWrapper httpContextWrapper, DateTime? expired)
		{
			try
			{
				using (var stream = httpContextWrapper.Request.GetBufferlessInputStream())
				{
					var isMultipart = Request.Content.IsMimeMultipartContent();
					if (isMultipart)
					{
						return await PostMultipartRequest(stream, expired);
					}
					else
					{
						return await PostNonMultipartRequest(stream, expired);
					}
				}
			}
			catch(Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		private async Task<IHttpActionResult> PostMultipartRequest(Stream stream, DateTime? expired)
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

				var chunk = await PostCompleteFile(fileName, fileType, mpp, expired);

				//move the stream foward until we get to the next part
				mpp = mpp.ReadUntilNextPart();
				if (mpp != null)
				{
					// Right now we are only supporting uploading the first part of multipart. Can easily change it to upload more than one.
					await _filesRepo.DeleteFile(chunk.FileId, DateTime.UtcNow);
					return BadRequest();
				}
				return Ok(Models.File.ConvertFileId(chunk.FileId));
			}

			return BadRequest();
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

		private async Task<IHttpActionResult> PostNonMultipartRequest(Stream stream, DateTime? expired)
		{
			if (string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition?.FileName) ||
					string.IsNullOrWhiteSpace(Request.Content.Headers.ContentType?.MediaType))
			{
				return BadRequest();
			}
			// Grabs all available information from the header
			var fileName = Request.Content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty).Replace("%20", " ");
			var fileMediaType = Request.Content.Headers.ContentType.MediaType;

			var chunk = await PostCompleteFile(fileName, fileMediaType, stream, expired);

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

        internal async Task<IHttpActionResult> PutFileHttpContext(string id, HttpContextWrapper httpContextWrapper)
	    {
            var fileId = Models.File.ConvertToStoreId(id);
            var fileHead = await _filesRepo.GetFileHead(fileId);
            if (fileHead == null)
            {
                return NotFound();
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
                    fileSize = await PostFileInChunks(stream, chunk);
                }
                await _filesRepo.UpdateFileHead(chunk.FileId, fileHead.FileSize + fileSize, chunk.ChunkNum - 1);
            }
            catch
            {
                // Delete all chunks after the starting chunk of this PUT.
                await DeleteFileChunks(chunk.FileId, startingChunkNumber);
                throw;
            }

            return Ok();
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


    }
}
