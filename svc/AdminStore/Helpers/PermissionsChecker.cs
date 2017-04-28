using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class PermissionsChecker
    {
        public static bool IsFlagBelongPermissions(InstanceAdminPrivileges permissions, InstanceAdminPrivileges instanceAdminPrivileges)
        {
            return (permissions & instanceAdminPrivileges) == instanceAdminPrivileges;
        }
    }
}