using System;
using System.ComponentModel.DataAnnotations;
using ServiceLibrary.Attributes;

namespace ServiceLibrary.Models.Enums
{
    [Flags]
    public enum InstanceAdminPrivileges
    {
        [Display(Name = "None", Description = "None", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_General")]
        None = 0x00,

        [Display(Name = "InstanceAdminPriviliges_AccessMainExperience_Key", Description = "InstanceAdminPriviliges_AccessMainExperience_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_General")]
        AccessMainExperience = 0x01,

        [PreRequiredPrivilege((int)AccessMainExperience)]
        [Display(Name = "InstanceAdminPriviliges_AccessAllProjectData_Key", Description = "InstanceAdminPriviliges_AccessAllProjectData_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_General")]
        AccessAllProjectData = 0x02 | AccessMainExperience | ViewProjects | ManageProjects | AccessAllProjectsAdmin,

        [Display(Name = "InstanceAdminPriviliges_ViewInstanceSettings_Key", Description = "InstanceAdminPriviliges_ViewInstanceSettings_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_InstanceSettings")]
        ViewInstanceSettings = 0x04,

        [Display(Name = "InstanceAdminPriviliges_ManageInstanceSettings_Key", Description = "InstanceAdminPriviliges_ManageInstanceSettings_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_InstanceSettings")]
        ManageInstanceSettings = 0x08 | ViewInstanceSettings,

        [Display(Name = "InstanceAdminPriviliges_ViewAdminRoles_Key", Description = "InstanceAdminPriviliges_ViewAdminRoles_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_AdminRoles")]
        ViewAdminRoles = 0x10,

        [Display(Name = "InstanceAdminPriviliges_ManageAdminRoles_Key", Description = "InstanceAdminPriviliges_ManageAdminRoles_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_AdminRoles")]
        ManageAdminRoles = 0x20 | ViewAdminRoles,

        [Display(Name = "InstanceAdminPriviliges_ViewProjects_Key", Description = "InstanceAdminPriviliges_ViewProjects_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_ProjectManagement")]
        ViewProjects = 0x40,

        [Display(Name = "InstanceAdminPriviliges_ManageProjects_Key", Description = "InstanceAdminPriviliges_ManageProjects_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_ProjectManagement")]
        ManageProjects = 0x80 | ViewProjects,

        [Display(Name = "InstanceAdminPriviliges_DeleteProjects_Key", Description = "InstanceAdminPriviliges_DeleteProjects_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_ProjectManagement")]
        DeleteProjects = 0x100 | ViewProjects | ManageProjects,

        [Display(Name = "InstanceAdminPriviliges_AccessAllProjectsAdmin_Key", Description = "InstanceAdminPriviliges_AccessAllProjectsAdmin_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_ProjectManagement")]
        AccessAllProjectsAdmin = 0x200 | ViewProjects | ManageProjects,

        [Display(Name = "InstanceAdminPriviliges_ViewUsers_Key", Description = "InstanceAdminPriviliges_ViewUsers_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_UsersAndGroups")]
        ViewUsers = 0x400,

        [Display(Name = "InstanceAdminPriviliges_ManageUsers_Key", Description = "InstanceAdminPriviliges_ManageUsers_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_UsersAndGroups")]
        ManageUsers = 0x800 | ViewUsers,

        [Display(Name = "InstanceAdminPriviliges_ViewGroups_Key", Description = "InstanceAdminPriviliges_ViewGroups_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_UsersAndGroups")]
        ViewGroups = 0x1000,

        [Display(Name = "InstanceAdminPriviliges_ManageGroups_Key", Description = "InstanceAdminPriviliges_ManageGroups_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_UsersAndGroups")]
        ManageGroups = 0x2000 | ViewGroups,

        [Display(Name = "InstanceAdminPriviliges_AssignAdminRoles_Key", Description = "InstanceAdminPriviliges_AssignAdminRoles_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_UsersAndGroups")]
        AssignAdminRoles = 0x4000 | ManageUsers | ManageGroups,

        [Display(Name = "InstanceAdminPriviliges_CanReportOnAllProjects_Key", Description = "InstanceAdminPriviliges_CanReportOnAllProjects_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_DataAnalytics")]
        CanReportOnAllProjects = 0x8000,

        [Display(Name = "InstanceAdminPriviliges_ViewStandardPropAndArtTypes_Key", Description = "InstanceAdminPriviliges_ViewStandardPropAndArtType_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_Standardization")]
        ViewStandardPropertiesAndArtifactTypes = 0x10000,

        [Display(Name = "InstanceAdminPriviliges_ManageStandardPropAndArtType_Key", Description = "InstanceAdminPriviliges_ManageStandardPropAndArtType_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_Standardization")]
        ManageStandardPropertiesAndArtifactTypes = 0x20000 | ViewStandardPropertiesAndArtifactTypes | AccessAllProjectsAdmin,

        [Display(Name = "InstanceAdminPriviliges_CanManageAllRunningJobs_Key", Description = "InstanceAdminPriviliges_CanManageAllRunningJobs_Desc", ResourceType = typeof(StringTokens), GroupName = "InstanceAdminPriviliges_Group_JobManagement")]
        CanManageAllRunningJobs = 0x40000
    }
}