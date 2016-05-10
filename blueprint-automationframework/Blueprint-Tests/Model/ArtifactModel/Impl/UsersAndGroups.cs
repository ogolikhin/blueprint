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
}