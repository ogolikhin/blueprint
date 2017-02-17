using System;
using Newtonsoft.Json;

namespace Model.Impl
{
    //class for object returned by adminstore/users/loginuser
    public class AdminStoreUser : User
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

        public override void CreateUser(UserSource source = UserSource.Database)
        {
            throw new NotImplementedException();
        }

        public override void DeleteUser(bool useSqlUpdate = false)
        {
            throw new NotImplementedException();
        }
    }
}
