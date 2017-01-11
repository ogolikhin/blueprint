
namespace Model.Impl
{
    // Copied from: blueprint/svc/lib/ServiceLibrary/Models/LicenseUsage.cs
    public class LicenseUsage
    {
        #region Properties

        public int ActivityMonth { get; set; }
        public int ActivityYear { get; set; }
        public int UniqueAuthors { get; set; }
        public int UniqueCollaborators { get; set; }
        public int MaxConCurrentViewers { get; set; }
        public int MaxConCurrentAuthors { get; set; }
        public int MaxConCurrentCollaborators { get; set; }
        public int UsersFromAnalytics { get; set; }
        public int UsersFromRestApi { get; set; }

        #endregion Properties
    }
}
