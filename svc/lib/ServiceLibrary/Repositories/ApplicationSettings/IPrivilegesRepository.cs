using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    public interface IPrivilegesRepository
    {
        Task<InstanceAdminPrivileges> GetInstanceAdminPrivilegesAsync(int userId);

        Task<ProjectAdminPrivileges> GetProjectAdminPermissionsAsync(int userId, int projectId);
    }
}
