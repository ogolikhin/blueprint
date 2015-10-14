using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Data.SqlTypes;
 
namespace FileStore.Repositories
{
	public class FileStreamAPI 
	{

        private  IFileStreamRepository _repo;

        public FileStreamAPI() : this(new FileStreamRepository())
        {
        }

        internal FileStreamAPI(IFileStreamRepository repo)
        {
            _repo = repo;

        }
        
        public async Task<Models.File> GetFile(Guid guid, string contentType)
        {
            // Note: using the streaming APIs exposed by SQL Server to 
            // retrieve the FileStream.  

            // if FILESTREAM data file is not found return null

            Models.File file = null;
            
            try
            {
                byte[] fileStream = await _repo.GetFileStreamAsync(guid);
                if (fileStream != null)
                {
                    file = new Models.File()
                    {
                        FileId = guid,
                        FileContent = fileStream,
                        FileSize = fileStream.Length,
                        FileType = contentType ?? "application/octet-stream"
                    };
                }
            }
            catch
            {
                // handle all exceptions upstream
                throw;
            }
            return file;
        }

    }
}