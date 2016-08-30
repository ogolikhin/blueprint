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
        /// <summary>
        /// The Id of the user or group.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The Display Name of the user or group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The E-mail address of the user or group.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// True if this is a group; false if this is a user.
        /// </summary>
        public bool IsGroup { get; set; }

        /// <summary>
        /// For users only.  Specifies if the user is a guest.
        /// </summary>
        public bool Guest { get; set; }

        /// <summary>
        /// Blocked means the user is locked out and cannot login.
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// For users only.  Specifies whether the user has an image.
        /// </summary>
        public bool HasImage { get; set; }
    }
}