using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Collections.Generic;
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
	    private const string StoredDateFormat = "o";

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
			try
			{
				var isMultipart = Request.Content.IsMimeMultipartContent();
				HttpContent content;
				if (isMultipart)
				{
					var multipartMemoryStreamProvider = await Request.Content.ReadAsMultipartAsync();
					if (multipartMemoryStreamProvider.Contents.Count > 1)
					{
						return BadRequest();
					}
					content = multipartMemoryStreamProvider.Contents.First();
				}
				else
				{
					//Temporarily allow only multipart uploads
					if (string.IsNullOrWhiteSpace(Request.Content.Headers.ContentDisposition?.FileName) ||
						 string.IsNullOrWhiteSpace(Request.Content.Headers.ContentType?.MediaType))
					{
						return BadRequest();
					}
					content = Request.Content;
				}
				var file = new Models.File
				{
					StoredTime = DateTime.UtcNow, // use UTC time to store data
					FileName = content.Headers.ContentDisposition.FileName.Replace("\"", string.Empty).Replace("%20", " "),
					FileType = content.Headers.ContentType.MediaType,
					FileSize = content.Headers.ContentLength.GetValueOrDefault(),
				};
				file.ChunkCount = (int)Math.Ceiling((double)file.FileSize / _configRepo.FileChunkSize);
				file.FileId = await _filesRepo.PostFileHead(file);
				var chunk = new Models.FileChunk
				{
					FileId = file.FileId,
                    ChunkNum = 1
				};
				using (var stream = await content.ReadAsStreamAsync())
				{
					var buffer = new byte[_configRepo.FileChunkSize];
					for (var remaining = file.FileSize; remaining > 0; remaining -= chunk.ChunkSize)
					{
							chunk.ChunkSize = (int)Math.Min(_configRepo.FileChunkSize, remaining);
						await stream.ReadAsync(buffer, 0, chunk.ChunkSize);
						chunk.ChunkContent = buffer.Take(chunk.ChunkSize).ToArray();
						chunk.ChunkNum = await _filesRepo.PostFileChunk(chunk);
					}
				}
				return Ok(Models.File.ConvertFileId(file.FileId));
			}
			catch
			{
				return InternalServerError();
			}
		}

		[HttpHead]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetFileHead(string id)
		{
			try
			{
                Models.File file = null;
                bool isLegacyFile = false;
                string mappedContentType = FileMapperRepository.DefaultMediaType;

                var fileId = Models.File.ConvertToStoreId(id);

                file = await _filesRepo.GetFileHead(fileId);
                 
                if (file == null)
				{
                    // if the file is not found in the FileStore check the
                    // legacy database for the file 

                    file = _fileStreamRepo.GetFileHead(fileId);
                    isLegacyFile = true;
                }
               
				if (file == null)
				{
                    // the file was not found in either FileStore or legacy database 
                    return NotFound();
				}

                if (isLegacyFile)
                {
                    mappedContentType = _fileMapperRepo.GetMappedOutputContentType(file.FileType);
				}
				else
				{
                    mappedContentType = file.FileType;
                }

                var response = Request.CreateResponse(HttpStatusCode.OK);

                response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("")); ;

                // return file info in headers

                response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
                response.Headers.Add(Pragma, NoCache); // HTTP 1.0.
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment) { FileName = file.FileName };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(mappedContentType);
                response.Content.Headers.ContentLength = file.FileSize;
                response.Headers.Add(StoredDate, file.StoredTime.ToString(StoredDateFormat));
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


        [HttpGet]
		[Route("{id}")]
		[ResponseType(typeof(HttpResponseMessage))]
		public async Task<IHttpActionResult> GetFileContent(string id)
		{
			try
			{
                Models.File file = null;
                bool isLegacyFile = false; 

                string mappedContentType = FileMapperRepository.DefaultMediaType;

                var fileId = Models.File.ConvertToStoreId(id);

				file = await _filesRepo.GetFileHead(fileId);

                if (file == null)
                {
                    // if the file is not found in the FileStore check the
                    // legacy database for the file 

                    file = _fileStreamRepo.GetFileHead(fileId);
                    isLegacyFile = true;
				}

                if (file == null)
				{
                    // the file was not found in either FileStore or legacy database 
                    return NotFound();
				}

                if (isLegacyFile)
				{
                    mappedContentType = _fileMapperRepo.GetMappedOutputContentType(file.FileType);
				}
				else
				{
                    mappedContentType = file.FileType;
                }

				var response = Request.CreateResponse(HttpStatusCode.OK);
                HttpContent responseContent = null;
				 
				if (isLegacyFile)
				{
                    // retrieve file content from legacy database 

                    responseContent = new StreamContent(_fileStreamRepo.GetFileContent(fileId), _configRepo.FileChunkSize);
				}
				else
				{
                    // retrieve file content from FileStore database 
                    responseContent = new StreamContent(_filesRepo.GetFileContent(fileId), _configRepo.FileChunkSize);

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
