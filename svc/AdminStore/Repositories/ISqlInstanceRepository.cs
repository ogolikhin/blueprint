using AdminStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AdminStore.Repositories
{
    public interface ISqlInstanceRepository
    {
        Task<InstanceItem> GetInstanceFolderAsync(int id);

        Task<List<InstanceItem>> GetInstanceFolderChildrenAsync(int id);
    }
}