using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminStore.Repositories
{
    public interface ISqlPrivilegesRepository
    {
        Task<bool> IsUserHasPermissions(IEnumerable<int> permissionsList, int userId);
    }
}