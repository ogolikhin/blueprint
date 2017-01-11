
namespace Model.Impl
{
    // Copied from: blueprint/svc/lib/ServiceLibrary/Models/LicenseUsage.cs
    public class LicenseUsage
    {
        #region Properties

        /// <summary>
        /// Month.
        /// </summary>
        public int ActivityMonth { get; set; }

        /// <summary>
        /// Year.
        /// </summary>
        public int ActivityYear { get; set; }

        /// <summary>
        /// Number of unique authors who have accessed BP in a given month
        /// </summary>

        public int UniqueAuthors { get; set; }

        /// <summary>
        /// Number of unique collaborators who have accessed BP in a given month
        /// </summary>
        public int UniqueCollaborators { get; set; }

        /// <summary>
        /// Maximum number of concurent Viewer licenses.
        /// </summary>
        public int MaxConcurrentViewers { get; set; }

        /// <summary>
        /// Maximum number of concurent Author licenses.
        /// </summary>
        public int MaxConcurrentAuthors { get; set; }

        /// <summary>
        /// Maximum number of concurent Collaborator licenses.
        /// </summary>
        public int MaxConcurrentCollaborators { get; set; }

        /// <summary>
        /// Number of users logged in from Analytics.
        /// </summary>
        public int UsersFromAnalytics { get; set; }

        /// <summary>
        /// Number of users logged in from RestAPI.
        /// </summary>
        public int UsersFromRestApi { get; set; }

        #endregion Properties
    }
}
