using System;
using System.Text.RegularExpressions;

namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]    // Production doesn't have a 0 value.  Use null instead.
    public enum InstanceAdminRole
    {
        AdministerALLProjects = 7,
        AssignInstanceAdministrators = 8,
        BlueprintAnalytics = 9,
        DefaultInstanceAdministrator = 1,
        Email_ActiveDirectory_SAMLSettings = 3,
        InstanceStandardsManager = 10,
        LogGatheringAndLicenseReporting = 2,
        ManageAdministratorRoles = 4,
        ProvisionProjects = 6,
        ProvisionUsers = 5
    }

    public static class InstanceAdminRoles
    {
        /// <summary>
        /// Converting instance admin role string into InstanceAdminRole enum
        /// </summary>
        /// <param name="adminRole">Instance admin string</param>
        /// <returns>InstanceAdminRole enum value</returns>
        public static InstanceAdminRole? ConvertStringToInstanceAdminRole(string adminRole)
        {
            if (adminRole == null)
            {
                return null;
            }

            string enumString = Regex.Replace(adminRole, @"[\s+]|,", "");

            return (InstanceAdminRole)Enum.Parse(typeof(InstanceAdminRole), enumString);
        }

        /// <summary>
        /// Converting InstanceAdminRole enum value into string
        /// </summary>
        /// <param name="role">InstanceAdminRole enum value</param>
        /// <returns>Instance admin role string</returns>
        public static string ConvertInstanceAdminRoleToString(InstanceAdminRole? role)
        {
            if (role == null)
            {
                return null;
            }

            // Creates pattern to replace capital letters with the new character and capital letter
            // from https://gist.github.com/rymoore99/9091263
            var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            var potentiallyWithUnderscores = r.Replace(role.ToString(), " ");

            return Regex.Replace(potentiallyWithUnderscores, " _", ",");
        }
    }
}
