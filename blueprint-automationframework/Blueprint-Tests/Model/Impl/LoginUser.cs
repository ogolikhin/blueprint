using Newtonsoft.Json;
using Model.Common.Enums;

namespace Model.Impl
{
    //class for object returned by adminstore/users/loginuser
    public class LoginUser
    {
        /// <summary>
        /// The users's ID.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The user's login.
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The name to display for this user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The authentication source of the user.
        /// </summary>
        public UserSource Source { get; set; }

        /// <summary>
        /// True if this user has accepted the EULA, false otherwise.
        /// </summary>
        [JsonProperty("EULAccepted")]
        public bool EULAAccepted { get; set; }

        /// <summary>
        /// The type of license of this user.
        /// </summary>
        public LicenseLevel LicenseType { get; set; }

        /// <summary>
        ///  True only for users who have logged in via SAML. False for other types of SSO
        /// </summary>
        public bool IsSso { get; set; }

        /// <summary>
        /// True if this user is allowed to fallback to non-SSO authentication.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public bool? AllowFallback { get; set; }

        /// <summary>
        /// The Instance Admin Role ID of this user, if any.
        /// </summary>
        public InstanceAdminRole? InstanceAdminRoleId { get; set; }

        /// <summary>
        /// The Instance Admin privileges of this user.
        /// </summary>
        public InstanceAdminPrivileges InstanceAdminPrivileges { get; set; }

        public LoginUser(string login, string firstName, string lastName, string displayName, string email,
            UserSource source, bool eulaAccepted, LicenseLevel license, bool isSso, bool? allowFallback, 
            InstanceAdminRole instanceAdminRole, InstanceAdminPrivileges instanceAdminPrivileges)
        {
            Login = login;
            FirstName = firstName;
            LastName = lastName;
            DisplayName = displayName;
            Email = email;
            Source = source;
            EULAAccepted = eulaAccepted;
            LicenseType = license;
            IsSso = isSso;
            AllowFallback = allowFallback;
            InstanceAdminRoleId = instanceAdminRole;
            InstanceAdminPrivileges = instanceAdminPrivileges;
        }
    }
}
