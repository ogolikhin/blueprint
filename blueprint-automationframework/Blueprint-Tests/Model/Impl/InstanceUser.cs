using Model.Common.Enums;

namespace Model.Impl
{
    //class for object returned by adminstore/users
    public class InstanceUser : LoginUser
    {
        /// <summary>
        /// The user's job title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The user's department.
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// True if the user is enabled, false otherwise.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// True if the user's password can expire, false otherwise.
        /// </summary>
        public bool ExpirePassword { get; set; }

        /// <summary>
        /// True if the user has membership in any groups, false otherwise.
        /// </summary>
        public bool HasGroups { get; set; }

        /// <summary>
        /// The current version of the user.
        /// </summary>
        public int CurrentVersion { get; set; }

        /// <summary>
        /// A new password for the user.
        /// (Only to be used for create/update user)
        /// </summary>
        public string NewPassword { get; set; }

        public InstanceUser(string login, string firstName, string lastName, string displayName, string email,
            UserSource source, bool eulaAccepted, LicenseLevel license, bool isSso, bool? allowFallback, 
            InstanceAdminRole instanceAdminRole, InstanceAdminPrivileges instanceAdminPrivileges,
            string title, string department, bool enabled, bool expirePassword, bool hasGroups, 
            int currentVersion, string newPassword = null)
            : base(login, firstName, lastName, displayName, email, source, eulaAccepted, license, isSso, 
                  allowFallback, instanceAdminRole, instanceAdminPrivileges)
        {
            Title = title;
            Department = department;
            Enabled = enabled;
            ExpirePassword = expirePassword;
            HasGroups = hasGroups;
            CurrentVersion = currentVersion;
            NewPassword = newPassword;
        }
    }
}
