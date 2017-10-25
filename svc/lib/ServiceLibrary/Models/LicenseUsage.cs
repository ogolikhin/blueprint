using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseUsage
    {
        public IEnumerable<LicenseUsageSummary> Summary { get; set; }
        public IEnumerable<LicenseUserActivity> UserActivities { get; set; }
    }


    /// <summary>
    /// The structure to keep calculated license user activities.
    /// </summary>
    /// <remarks>
    /// Populated from LicenseActivities tables.
    /// </remarks>
    [JsonObject]
    public class LicenseUserActivity
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// License type being used by user
        /// </summary>
        public int LicenseType { get; set; }
        /// <summary>
        /// Composite Year and Month for user activity
        /// </summary>
        public int YearMonth { get; set; }

    }

    /// <summary>
    /// The structure to keep calculated license usage activities.
    /// </summary>
    /// <remarks>
    /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
    /// </remarks>
    [JsonObject]
    public class LicenseUsageSummary
    {
        /// <summary>
        /// Composite Year and Month of license usage
        /// </summary>
        public int YearMonth { get; set; }

        /// <summary>
        /// Number of unique authors who have accessed BP in a given month
        /// </summary>
        public int UniqueAuthors { get; set; }

        /// <summary>
        /// Number of unique collaborators who have accessed BP in a given month
        /// </summary>
        public int UniqueCollaborators { get; set; }

        /// <summary>
        /// Number of unique viewers who have accessed BP in a given month
        /// </summary>
        public int UniqueViewers { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Author licenses.
        /// </summary>
        public int MaxConcurrentAuthors { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Collaborator licenses.
        /// </summary>
        public int MaxConcurrentCollaborators { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Viewer licenses.
        /// </summary>
        public int MaxConcurrentViewers { get; set; }

        /// <summary>
        /// Number of users logged in from Analytics.
        /// </summary>
        public int UsersFromAnalytics { get; set; }

        /// <summary>
        /// Number of users logged in from RestAPI.
        /// </summary>
        public int UsersFromRestApi { get; set; }

    }
}
