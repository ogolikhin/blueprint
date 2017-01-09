
namespace Model.Impl
{
    public class LicenseUsageInfo : ILicenseUsageInfo
    {
        #region Properties

        public int activityMonth { get; set; }
        public int activityYear { get; set; }
        public int authorsActive { get; set; }
        public int authorsActiveLoggedOn { get; set; }
        public int authorsCreatedtodate { get; set; }
        public int collaboratorsActive { get; set; }
        public int collaboratorsActiveLoggedOn { get; set; }
        public int collaboratorsActivetodate { get; set; }
        public int maxConCurrentAuthors { get; set; }
        public int maxConCurrentCollaborators { get; set; }
        public int maxConCurrentViewers { get; set; }
        public int registeredAuthorsCreated { get; set; }
        public int registeredCollaboratorsCreated { get; set; }
        public int uniqueAuthors { get; set; }
        public int uniqueCollaborators { get; set; }
        public int uniqueViewers { get; set; }
        public int usersFromAnalytics { get; set; }
        public int usersFromRestApi { get; set; }

        #endregion Properties
    }
}
