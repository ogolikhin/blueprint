using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class UserPermissionsValidator
    {
        public static async Task<bool> HasValidPermissions(int sessionUserId, UserDto user, IPrivilegesRepository sqlPrivilegesRepository)
        {
            var userPermissions = await sqlPrivilegesRepository.GetInstanceAdminPrivilegesAsync(sessionUserId);
            if (!PermissionsChecker.IsFlagBelongPermissions(userPermissions, InstanceAdminPrivileges.ManageUsers))
                return false;

            if (user.InstanceAdminRoleId.HasValue && (!PermissionsChecker.IsFlagBelongPermissions(userPermissions, InstanceAdminPrivileges.AssignAdminRoles)))
            {
                return false;
            }
            return true;
        }
    }
}