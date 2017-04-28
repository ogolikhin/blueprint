using System.Collections.Generic;

namespace AdminStore.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public UserGroupSource Source { get; set; }
        public int LicenseType { get; set; }
        public bool? AllowFallback { get; set; }
        public int? InstanceAdminRoleId { get; set; }       
        public bool Guest { get; set; }
        public int CurrentVersion { get; set; }
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public bool? ExpirePassword { get; set; }
        public int? Image_ImageId { get; set; }
        public IEnumerable<int> GroupMembership { get; set; }
        public string Password { get; set; }
    }
}