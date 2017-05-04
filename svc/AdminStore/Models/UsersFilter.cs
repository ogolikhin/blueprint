namespace AdminStore.Models
{
    public class UsersFilter
    {
        public string Login { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string Department { get; set; }
        public int LicenseType { get; set; }
        public bool Enabled { get; set; }
        public int? InstanceAdminRoleId { get; set; }
        public UserGroupSource Source { get; set; }
    }
}