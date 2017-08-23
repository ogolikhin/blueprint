using AdminStore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminStore.Services.Instance
{
    public interface IInstanceService
    {
        Task<IEnumerable<InstanceItem>> GetFoldersByName(string name);
    }
}
