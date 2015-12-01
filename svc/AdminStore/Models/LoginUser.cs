using System;
using System.Runtime.Serialization;

namespace AdminStore.Models
{
    public class LoginUser
    {
        public int Id { get; set; }

        public string Login { get; set; }

        [IgnoreDataMember]
        public string Password { get; set; }

        [IgnoreDataMember]
        public bool IsEnabled { get; set; }

        public UserGroupSource Source { get; set; }

        [IgnoreDataMember]
        public int InvalidLogonAttemptsNumber { get; set; }

        [IgnoreDataMember]
        public DateTime? LastInvalidLogonTimeStamp { get; set; }

        [IgnoreDataMember]
        public Guid UserSalt { get; set; }

        public bool? IsFallbackAllowed { get; set; }

        [IgnoreDataMember]
        public DateTime? LastPasswordChangeTimestamp { get; set; }

        [IgnoreDataMember]
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