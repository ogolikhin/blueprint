using System;
using Newtonsoft.Json;

namespace Model
{
    //class for object returned by adminstore/users/loginuser
    public class AdminStoreUser
    {
        [JsonProperty("Login")]
        public string Username { get; set; }//"Login" field in database.

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Source")]
        public UserSource Source { get; }

        [JsonProperty("LicenseType")]
        public LicenseType License { get; set; }

        [JsonProperty("InstanceAdminRoleId")]
        public InstanceAdminRole InstanceAdminRole { get; set; }

        public AdminStoreUser(string username, string firstName, string lastName, string displayName, string email,
            UserSource source, LicenseType license, InstanceAdminRole instanceAdminRole)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            DisplayName = displayName;
            Email = email;
            Source = source;
            License = license;
            InstanceAdminRole = instanceAdminRole;
        }
        public AdminStoreUser()
        { }

        /// <summary>
        /// Tests whether the specified IUser is equal to this one.
        /// </summary>
        /// <param name="user">The User to compare.</param>
        /// <returns>True if the sessions are equal, otherwise false.</returns>
        public bool Equals(IUser user)///TODO: add compare for license
        {
            if (user == null)
                return false;
            else
                return ((this.Username == user.Username) & (this.DisplayName == user.DisplayName) &&
                    (this.Email == user.Email) && (this.FirstName == user.FirstName) &&
                    (this.InstanceAdminRole == user.InstanceAdminRole) && (this.LastName == user.LastName)
                    && (this.Source == user.Source));
        }
    }
}
