

using System.Collections.Generic;
using AdminStore.Models.DTO;

namespace AdminStore.Services.Instance
{
    public interface IInstanceService
    {
        IEnumerable<FolderDto> GetFoldersByName(string name);
    }
}
