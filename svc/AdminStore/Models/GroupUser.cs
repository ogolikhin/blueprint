using AdminStore.Models.Enums;

namespace AdminStore.Models
{
    public class GroupUser
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public UserType Type { get; set; }
        public string Scope { get; set; }
        public LicenseType LicenseType { get; set; }
        public UserGroupSource Source { get; set; }
    }
}