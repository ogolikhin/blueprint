using System.Collections.Generic;

namespace Model.Common.Enums
{
    public static class InstanceAdminRoleExtensions
    {
        /// <summary>
        /// This is a map of InstanceAdminRole enum values to InstanceAdminRole enum names.
        /// </summary>
        internal static Dictionary<InstanceAdminRole?, string> InstanceAdminRoleStringMap { get; } =
            new Dictionary<InstanceAdminRole?, string>
        {
            {InstanceAdminRole.AdministerALLProjects,               "Administer ALL Projects"},
            {InstanceAdminRole.AssignInstanceAdministrators,        "Assign Instance Administrators"},
            {InstanceAdminRole.BlueprintAnalytics,                  "Blueprint Analytics"},
            {InstanceAdminRole.DefaultInstanceAdministrator,        "Default Instance Administrator"},
            {InstanceAdminRole.Email_ActiveDirectory_SAMLSettings,  "Email, Active Directory, SAML Settings"},
            {InstanceAdminRole.InstanceStandardsManager,            "Instance Standards Manager"},
            {InstanceAdminRole.LogGatheringAndLicenseReporting,     "Log Gathering and License Reporting"},
            {InstanceAdminRole.ManageAdministratorRoles,            "Manage Administrator Roles"},
            {InstanceAdminRole.ProvisionProjects,                   "Provision Projects"},
            {InstanceAdminRole.ProvisionUsers,                      "Provision Users"}
        };

        /// <summary>
        /// Converts this InstanceAdminRole enum value to its InstanceAdminRole string equivalent.
        /// </summary>
        /// <param name="instanceAdminRole">The InstanceAdminRole to convert.</param>
        /// <returns>The string version of this InstanceAdminRole.</returns>
        public static string ToInstanceAdminRoleString(this InstanceAdminRole? instanceAdminRole)
        {
            return InstanceAdminRoleStringMap[instanceAdminRole];
        }

        /// <summary>
        /// This is a map of InstanceAdminRole enum values to InstanceAdminRole enum names.
        /// </summary>
        internal static Dictionary<string, InstanceAdminRole?> StringInstanceAdminRoleMap { get; } =
            new Dictionary<string, InstanceAdminRole?>
        {
            {"Administer ALL Projects",                 InstanceAdminRole.AdministerALLProjects},
            {"Assign Instance Administrators",          InstanceAdminRole.AssignInstanceAdministrators},
            {"Blueprint Analytics",                     InstanceAdminRole.BlueprintAnalytics},
            {"Default Instance Administrator",          InstanceAdminRole.DefaultInstanceAdministrator},
            {"Email, Active Directory, SAML Settings",  InstanceAdminRole.Email_ActiveDirectory_SAMLSettings},
            {"Instance Standards Manager",              InstanceAdminRole.InstanceStandardsManager},
            {"Log Gathering and License Reporting",     InstanceAdminRole.LogGatheringAndLicenseReporting},
            {"Manage Administrator Roles",              InstanceAdminRole.ManageAdministratorRoles},
            {"Provision Projects",                      InstanceAdminRole.ProvisionProjects},
            {"Provision Users",                         InstanceAdminRole.ProvisionUsers},
        };

        /// <summary>
        /// Converts this InstanceAdminRole enum string to its InstanceAdminRole value equivalent.
        /// </summary>
        /// <param name="instanceAdminRole">The InstanceAdminRole to convert.</param>
        /// <returns>The InstanceAdminRole version of this string.</returns>
        public static InstanceAdminRole? ToInstanceAdminRoleValue(this string instanceAdminRole)
        {
            return StringInstanceAdminRoleMap[instanceAdminRole];
        }
    }
}

