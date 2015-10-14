using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileStore.Models;

namespace FileStore.Repositories
{
	public interface IFileStreamRepository
	{
        Task<byte[]> GetTSqlFileStreamAsync(Guid guid);
        Task<byte[]> GetFileStreamAsync(Guid guid);
    }
}
