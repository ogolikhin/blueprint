using System.Collections.Generic;
using System.Threading.Tasks;
using AdminStore.Models;

namespace AdminStore.Repositories
{
    public interface ISqlPrivilegesRepository
    {
        Task<int> GetUserPermissionsAsync(int userId);
    }
}