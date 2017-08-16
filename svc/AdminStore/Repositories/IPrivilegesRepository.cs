using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceLibrary.Models;

namespace AdminStore.Repositories
{
    public interface IPrivilegesRepository
    {
        Task<InstanceAdminPrivileges> GetInstanceAdminPrivilegesAsync(int userId);

        Task<bool> HasUserInstanceOrProjectPermissionsForProject(int userId, int projectId,
            InstanceAdminPrivileges instancePrivileges, ProjectAdminPrivileges projectPrivileges);
    }
}