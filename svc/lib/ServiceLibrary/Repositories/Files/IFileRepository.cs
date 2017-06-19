using System;
using System.IO;
using System.Threading.Tasks;
using ServiceLibrary.Models.Files;

namespace ServiceLibrary.Repositories.Files
{
    public interface IFileRepository
    {
        Task<Models.Files.File> GetFileAsync(Guid fileId);
        Task<string> UploadFileAsync(string fileName, string fileType, Stream content, DateTime? expired = null);
    }
}
