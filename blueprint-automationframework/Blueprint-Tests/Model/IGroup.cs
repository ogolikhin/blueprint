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
        void AddGroupToDatabase();

        /// <summary>
        /// 
        /// </summary>
        void AddUser(IUser user);

        void DeleteGroup();

        void AssignProjectAuthorRole(IProject project);
        #endregion Methods
    }
}
