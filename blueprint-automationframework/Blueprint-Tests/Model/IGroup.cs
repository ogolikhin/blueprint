using System.Collections.Generic;

namespace Model
{
    public enum GroupLicenseType
    {
        Author,
        Collaborate
    }

    public enum GroupSource
    {
        Database,
        Windows
    }


    public interface IGroup
    {
        #region Properties

        string Email { get; set; }
        bool LicenseGroup { get; set; }
        GroupLicenseType LicenseType { get; set; }
        string Name { get; set; }
        List<IUser> Members { get; }
        IProject Scope { get; set; }
        GroupSource Source { get; set; }

        #endregion Properties
    }
}
