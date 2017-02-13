using System.Collections.Generic;

namespace Model.Impl
{
    public class UserDataModel
    {
        #region Properties

        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public string Password { get; set; }
        public List<IGroup> GroupMembership { get; set; }
        public InstanceAdminRole? InstanceAdminRole { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Enabled { get; set; }

        /*
        public bool IsDeleted { get { return (!IsDeletedFromDatabase && (EndTimestamp != null)); } }

        // All User table fields are as follows:
        // [AllowFallback],[CurrentVersion],[Department],[DisplayName],[Email],[Enabled],[EndTimestamp],[EULAccepted],[ExpirePassword],[FirstName],[Guest],[Image_ImageId],[InstanceAdminRoleId],
        // [InvalidLogonAttemptsNumber],[LastInvalidLogonTimeStamp],[LastName],[LastPasswordChangeTimestamp],[Login],[Password],[Source],[StartTimestamp],[Title],[UserId],[UserSALT]
        public LicenseType License { get; set; }
        public IEnumerable<byte> Picture { get; set; }
        public virtual UserSource Source { get { return UserSource.Unknown; } }
        public IBlueprintToken Token { get; set; } = new BlueprintToken();

        // These are fields not included by IUser:
        public bool? AllowFallback { get; set; }
        public int CurrentVersion { get; set; }
        public string EncryptedPassword { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public bool EULAccepted { get; set; }
        public bool Guest { get; set; }
        public int InvalidLogonAttemptsNumber { get; set; }
        public DateTime? LastInvalidLogonTimeStamp { get; set; }
        public DateTime? LastPasswordChangeTimestamp { get; set; }
        public DateTime StartTimestamp { get; set; }
        public Guid UserSALT { get; set; }
        */
        #endregion Properties
    }
}
