
namespace Model.Impl
{
    // Copied from: blueprint/svc/lib/ServiceLibrary/Models/LicenseUsage.cs
    public class LicenseUsage// : ILicenseUsageInfo
    {
        #region Properties

        public int ActivityMonth { get; set; }
        public int ActivityYear { get; set; }
        public int UniqueViewers { get; set; }
        public int UniqueAuthors { get; set; }
        public int UniqueCollaborators { get; set; }
        public int MaxConCurrentViewers { get; set; }
        public int MaxConCurrentAuthors { get; set; }
        public int MaxConCurrentCollaborators { get; set; }
        public int UsersFromAnalytics { get; set; }
        public int UsersFromRestApi { get; set; }
        public int RegisteredAuthorsCreated { get; set; }
        public int RegisteredCollaboratorsCreated { get; set; }
        public int AuthorsCreatedtodate { get; set; }
        public int CollaboratorsCreatedtodate { get; set; }
        public int AuthorsActive { get; set; }
        public int AuthorsActiveLoggedOn { get; set; }
        public int CollaboratorsActive { get; set; }
        public int CollaboratorsActiveLoggedOn { get; set; }

        #endregion Properties
    }
}
