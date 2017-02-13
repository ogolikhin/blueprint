using System.Collections.Generic;

namespace Model
{
    interface IOpenApiUser
    {
        #region Properties

        int Id { get; set; }
        string Username { get; set; }
        string DisplayName { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        string Title { get; set; }
        string Department { get; set; }
        string Password { get; set; }
        List<IGroup> GroupMembership { get; set; }
        InstanceAdminRole? InstanceAdminRole { get; set; }
        bool? ExpirePassword { get; set; }
        bool Enabled { get; set; }

        #endregion Properties
    }
}
