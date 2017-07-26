using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models.DTO;

namespace AdminStore.Services.Instance
{
    public interface IInstanceService
    {
        Task<IEnumerable<FolderDto>> GetFoldersByName(string name);
    }
}
