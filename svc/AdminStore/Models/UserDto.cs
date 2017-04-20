using System.Diagnostics.CodeAnalysis;

namespace AdminStore.Models
{
    public class UserDto
    {
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public UserGroupSource Source { get; set; }
        public string LicenseType { get; set; }
        public bool? AllowFallback { get; set; }
        public int? InstanceAdminRoleId { get; set; }
        public int UserId { get; set; }
        public bool Guest { get; set; }
        public int CurrentVersion { get; set; }
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public bool? ExpirePassword { get; set; }
        public int? Image_ImageId { get; set; }
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public int[] GroupMembership { get; set; }
        public string Password { get; set; }
    }
}