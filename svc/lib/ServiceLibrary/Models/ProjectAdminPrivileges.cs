using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    [Flags]
    public enum ProjectAdminPrivileges
    {
        None = 0x00,

        ViewGroupsAndRoles = 0x01,

        ManageGroupsAndRoles = 0x02 | ViewGroupsAndRoles,

        ViewProjectConfiguration = 0x04,

        ManageProjectConfiguration = 0x08 | ViewProjectConfiguration,

        ViewAlmIntegration = 0x10,

        ManageAlmIntegration = 0x20 | ViewAlmIntegration
    }
}
