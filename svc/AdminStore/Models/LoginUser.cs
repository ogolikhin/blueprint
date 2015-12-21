using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class LoginUser
    {
        public int Id { get; set; }

        public string Login { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public bool IsEnabled { get; set; }

        public UserGroupSource Source { get; set; }

        [JsonIgnore]
        public int InvalidLogonAttemptsNumber { get; set; }

        [JsonIgnore]
        public DateTime? LastInvalidLogonTimeStamp { get; set; }

        [JsonIgnore]
        public Guid UserSalt { get; set; }

        public bool? IsFallbackAllowed { get; set; }

        [JsonIgnore]
        public DateTime? LastPasswordChangeTimestamp { get; set; }

        [JsonIgnore]
        public bool? ExpirePassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public int? InstanceAdminRoleId { get; set; }

        public int InstanceAdminPrivileges { get; set; }

        public bool EULAccepted { get; set; }

        public bool IsSso { get; set; }

        public int LicenseType { get; set; }
    }
}