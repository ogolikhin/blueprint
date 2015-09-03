using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repo
{
    public interface IRepo
    {
        Task<bool> AddFile(File file);
        Task<File> GetFileInfo(Guid fileId);
        Task<File> GetFile(Guid fileId);
        Task<bool> DeleteFile(Guid fileId);
    }
}
