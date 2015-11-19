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
		private readonly IConfigRepository _configRepo;

		private const string CacheControl = "Cache-Control";
		private const string Pragma = "Pragma";
		private const string StoredDate = "Stored-Date";
		private const string FileSize = "File-Size";
		private const string Attachment = "attachment";
		private const string NoCache = "no-cache";
		private const string NoStore = "no-store";
		private const string MustRevalidate = "must-revalidate";

		public FilesController() : this(new SqlFilesRepository(), new FileStreamRepository(), new FileMapperRepository(), new ConfigRepository())
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
		[Route("post")]
		[ResponseType(typeof(string))]
		public async Task<IHttpActionResult> PostFile()
		{
			try
			{
				var isMultipart = Request.Content.IsMimeMultipartContent();
				HttpContent content = null;
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
					ChunkNum = 0
				};
				using (var stream = await content.ReadAsStreamAsync())
				{
					for (var remaining = file.FileSize; remaining > 0; remaining -= chunk.ChunkSize)
					{
						using (var memoryStream = new MemoryStream())
						{
							chunk.ChunkSize = (int)Math.Min(_configRepo.FileChunkSize, remaining);
							stream.CopyTo(memoryStream, chunk.ChunkSize);
							chunk.ChunkContent = memoryStream.ToArray();
						}
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

				var guid = Models.File.ConvertToStoreId(id);

				var isFileStoreGuid = true;
				if (isHead)
				{
					file = await _filesRepo.GetFileHead(guid);
					if (file == null)
					{
						file = _fileStreamRepo.HeadFile(guid);
						isFileStoreGuid = false;
					}
				}
				else
				{
					file = await _filesRepo.GetFileHead(guid);
					if (file == null)
					{
						file = _fileStreamRepo.GetFile(guid);
						isFileStoreGuid = false;
					}
				}

				if (file == null || (!isFileStoreGuid && string.IsNullOrEmpty(file.FileName)))
				{
					return NotFound();
				}

				var mappedContentType = isFileStoreGuid ? file.FileType : _fileMapperRepo.GetMappedOutputContentType(file.FileType);
				if (string.IsNullOrWhiteSpace(mappedContentType))
				{
					mappedContentType = FileMapperRepository.DefaultMediaType;
				}

				var response = Request.CreateResponse(HttpStatusCode.OK);
                HttpContent responseContent = null;
				if (isHead)
				{
					responseContent = new ByteArrayContent(Encoding.UTF8.GetBytes(""));
				}
				else
				{
					if (isFileStoreGuid)
					{
						// TODO: fix
                        //responseContent = new ByteArrayContent(file.FileContent);
					}
					else
					{
						//responseContent = new StreamContent(file.FileStream, 1048576);
					}
				}

				response.Content = responseContent;

				response.Headers.Add(CacheControl, string.Format("{0}, {1}, {2}", NoCache, NoStore, MustRevalidate)); // HTTP 1.1.
				response.Headers.Add(Pragma, NoCache); // HTTP 1.0.
				response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(Attachment) { FileName = file.FileName };
				response.Content.Headers.ContentType = new MediaTypeHeaderValue(mappedContentType);
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
	}
}
