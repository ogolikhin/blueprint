using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceLibrary.Models;

namespace AdminStore.Helpers
{
    public class PermissionsChecker
    {
        public static bool IsFlagBelongPermissions(int permissions, InstanceAdminPrivileges instanceAdminPrivileges)
        {
            return (permissions & (int) instanceAdminPrivileges) == (int) instanceAdminPrivileges;
        }
    }
}