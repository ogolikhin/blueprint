using System;
using System.Threading.Tasks;
using ServiceLibrary.Models.Files;

namespace ServiceLibrary.Repositories
{
    public interface IFileRepository
    {
        Task<FileInfo> GetFileInfoAsync(Guid fileId, string sessionToken = null, int? timeout = null);

        Task<File> GetFileAsync(Guid fileId, string sessionToken = null);
    }
}
