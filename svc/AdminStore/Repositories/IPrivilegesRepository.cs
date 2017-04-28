using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface IPrivilegesRepository
    {
        Task<bool> IsUserHasPermissions(IEnumerable<int> permissionsList, int userId);

        Task<InstanceAdminPrivileges> GetInstanceAdminPrivilegesAsync(int userId);
    }
}