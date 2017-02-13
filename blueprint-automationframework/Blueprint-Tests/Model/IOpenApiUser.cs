using System.Collections.Generic;

namespace Model
{
    public interface IOpenApiUser
    {
        #region Properties

        int Id { get; set; }
        string Username { get; }
        string DisplayName { get; }
        string FirstName { get; }
        string LastName { get; }
        string Email { get; }
        string Title { get; }
        string Department { get; }
        string Password { get; }
        List<IGroup> GroupMembership { get; }
        InstanceAdminRole? InstanceAdminRole { get; }
        bool? ExpirePassword { get; }
        bool Enabled { get; }

        #endregion Properties
    }
}
