using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    [JsonObject]
    public class LicenseUsage
    {

        /// <summary>
        /// Month.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int ActivityMonth { get; set; }
        /// <summary>
        /// Year.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        /// 
        public int ActivityYear { get; set; }
        /// <summary>
        /// Number of unique Author licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int UniqueViewers { get; set; }

        /// <summary>
        /// Number of unique Author licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int UniqueAuthors { get; set; }
        
        /// <summary>
        /// Number of unique Author licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int UniqueCollaborators { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Viewer licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int MaxConCurrentViewers { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Author licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int MaxConCurrentAuthors { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Collaborator licenses.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int MaxConCurrentCollaborators { get; set; }
        
        /// <summary>
        /// Number of users logged in from Analytics.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int UsersFromAnalytics { get; set; }

        /// <summary>
        /// Number of users logged in from RestAPI.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int UsersFromRestApi { get; set; }
    }
}
