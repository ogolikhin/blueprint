using System;
using Newtonsoft.Json;

namespace Model
{
    //class for object returned by adminstore/users/loginuser
    public class AdminStoreUser : Impl.User
    {
        [JsonProperty("Login")]
        public new string Username { get; set; }//"Login" field in database.

        [JsonProperty("FirstName")]
        public new string FirstName { get; set; }

        [JsonProperty("LastName")]
        public new string LastName { get; set; }

        [JsonProperty("DisplayName")]
        public new string DisplayName { get; set; }

        [JsonProperty("Email")]
        public new string Email { get; set; }

        [JsonProperty("Source")]
        public new UserSource Source { get; }

        [JsonProperty("LicenseType")]
        public new LicenseType License { get; set; }

        [JsonProperty("InstanceAdminRoleId")]
        public new InstanceAdminRole InstanceAdminRole { get; set; }

        public AdminStoreUser(string username, string firstName, string lastName, string displayName, string email,
            LicenseType license, InstanceAdminRole instanceAdminRole)
        {
            Username = username;
            FirstName = firstName;
            LastName = lastName;
            DisplayName = displayName;
            Email = email;
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
            { 
                return false;
            }
            else
                return ((this.Username == user.Username) & (this.DisplayName == user.DisplayName) &&
                    (this.Email == user.Email) && (this.FirstName == user.FirstName) &&
                    (this.InstanceAdminRole == user.InstanceAdminRole) && (this.LastName == user.LastName)
                    && (this.Source == user.Source));
        }

        public override void CreateUser(UserSource source = UserSource.Database)
        {
            throw new NotImplementedException();
        }

        public override void DeleteUser(bool deleteFromDatabase = false)
        {
            throw new NotImplementedException();
        }
    }
}
