namespace Model.ArtifactModel.Impl
{
    /// <summary>
    /// An Enumeration of possible UsersAndGroups Types
    /// </summary>
    public enum UsersAndGroupsType
    {
        User,
        Group
    }

    public class UsersAndGroups
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public UsersAndGroupsType Type { get; set; }

        public int Id { get; set; }

        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Contains User or Group info.  Taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Models/Metadata/UserOrGroupInfo.cs
    /// </summary>
    public class UserOrGroupInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsGroup { get; set; }

        public bool Guest { get; set; }

        public bool IsBlocked { get; set; }

        public bool HasImage { get; set; }
    }
}