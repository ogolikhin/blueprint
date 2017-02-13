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
        public UserSource Source { get; set; }
        public List<IGroup> GroupMembership { get; set; }
        public InstanceAdminRole? InstanceAdminRole { get; set; }
        public bool? ExpirePassword { get; set; }
        public bool Enabled { get; set; }

        #endregion Properties
    }
}
