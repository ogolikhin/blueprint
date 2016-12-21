using Newtonsoft.Json;

namespace AdminStore.Models
{
	[JsonObject]
	public class LoginUser // : BluePrintSys.RC.Client.SL.Core.ILoginUser
    {
        /// <summary>
        /// The users's ID.
        /// </summary>
        public int Id { get; set; }

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
        public UserGroupSource Source { get; set; }

        /// <summary>
        /// True if this user has accepted the EULA, false otherwise.
        /// </summary>
        public bool EULAccepted { get; set; }

        /// <summary>
        /// The type of license of this user.
        /// </summary>
        public int LicenseType { get; set; }

        /// <summary>
        /// True if this is an SSO user, false otherwise.
        /// </summary>
        public bool IsSso { get; set; }

        /// <summary>
        /// True if this user is allowed to fallback to non-SSO authentication.
        /// </summary>
        public bool? AllowFallback { get; set; }

        /// <summary>
        /// The Instance Admin Role ID of this user, if any.
        /// </summary>
        public int? InstanceAdminRoleId { get; set; }

        /// <summary>
        /// The Instance Admin privliges of this user.
        /// </summary>
        public int InstanceAdminPrivileges { get; set; }
    }
}
