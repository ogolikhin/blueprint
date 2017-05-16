using System;
using ServiceLibrary.Attributes;

namespace ServiceLibrary.Models
{
    [Flags]
    public enum InstanceAdminPrivileges
    {
        None = 0x00,

        AccessMainExperience = 0x01,

        [PreRequiredPrivilege((int)AccessMainExperience)]
        AccessAllProjectData = 0x02 | AccessMainExperience | ViewProjects | ManageProjects | AccessAllProjectsAdmin,

        ViewInstanceSettings = 0x04,

        ManageInstanceSettings = 0x08 | ViewInstanceSettings,

        ViewAdminRoles = 0x10,

        ManageAdminRoles = 0x20 | ViewAdminRoles,

        ViewProjects = 0x40,

        ManageProjects = 0x80 | ViewProjects,

        DeleteProjects = 0x100 | ViewProjects | ManageProjects,

        AccessAllProjectsAdmin = 0x200 | ViewProjects | ManageProjects,

        ViewUsers = 0x400,

        ManageUsers = 0x800 | ViewUsers,

        ViewGroups = 0x1000,

        ManageGroups = 0x2000 | ViewGroups,

        AssignAdminRoles = 0x4000 | ManageUsers | ManageGroups,

        CanReportOnAllProjects = 0x8000,

        ViewStandardPropertiesAndArtifactTypes = 0x10000,

        ManageStandardPropertiesAndArtifactTypes = 0x20000 | ViewStandardPropertiesAndArtifactTypes | AccessAllProjectsAdmin,

        CanManageAllRunningJobs = 0x40000
    }
}