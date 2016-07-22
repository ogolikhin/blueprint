using System.Collections.Generic;

namespace Model
{
    public enum GroupLicenseType
    {
        None = 0,
        Author = 3,
        Collaborate = 2
    }

    public enum GroupSource
    {
        Database,
        Windows
    }


    public interface IGroup
    {
        #region Properties
        int GroupId { get; set; }

        string Name { get; set; }

        string Description { get; set; }

        string Email { get; set; }

        GroupSource Source { get; set; }

        GroupLicenseType LicenseType { get; set; }

        IProject Scope { get; set; }

        IGroup Parent { get; set; }

        bool IsLicenseGroup { get; set; }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Adds Group to the Blueprint database.
        /// </summary>
        void AddGroupToDatabase();

        /// <summary>
        /// Adds user to the Group.
        /// </summary>
        /// <param name="user">User to add to the Group.</param>
        void AddUser(IUser user);

        /// <summary>
        /// Deletes Groups from Blueprint database (updates Group related records in RoleAssignmet table,
        /// deletes from User_Groups tables, set EndTimestamp in Groups table). 
        /// </summary>
        void DeleteGroup();

        /// <summary>
        /// Assigns Author role in the specified project to the Group.
        /// </summary>
        /// <param name="project">Project in which role assignment will be created.</param>
        void AssignProjectAuthorRole(IProject project);
        #endregion Methods
    }
}
