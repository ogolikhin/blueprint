using Newtonsoft.Json;

namespace AdminStore.Models
{
    [JsonObject]
    public class LoginUser // : BluePrintSys.RC.Client.SL.Core.ILoginUser
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public UserGroupSource Source { get; set; }
        public bool EULAccepted { get; set; }
        public int LicenseType { get; set; }
        public bool IsSso { get; set; }
        public bool? AllowFallback { get; set; }
        public int? InstanceAdminRoleId { get; set; }
        public int InstanceAdminPrivileges { get; set; }
    }
}
