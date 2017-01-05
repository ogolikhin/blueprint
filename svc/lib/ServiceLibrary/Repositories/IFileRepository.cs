using System;
using System.Threading.Tasks;
using ServiceLibrary.Models.Files;

namespace ServiceLibrary.Repositories
{
    public interface IFileRepository
    {
        Task<FileInfo> GetFileInfoAsync(Uri baseUri, Guid fileId, string sessionToken = null, int? timeout = null);

        Task<File> GetFileAsync(Uri baseUri, Guid fileId, string sessionToken = null);
    }
}
