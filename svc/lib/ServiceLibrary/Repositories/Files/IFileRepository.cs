using System;
using System.Threading.Tasks;
using ServiceLibrary.Models.Files;

namespace ServiceLibrary.Repositories.Files
{
    public interface IFileRepository
    {
        Task<File> GetFileAsync(Guid fileId);
    }
}
