using System;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Files;

namespace ServiceLibrary.Repositories
{
    public interface IFileRepository
    {
        Task<File> GetFileAsync(Uri baseAddress, Guid fileId, string sessionToken, int timeout = ServiceConstants.DefaultRequestTimeout);
    }
}
