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


        /// <summary>
        /// Breaks out the Authors, may be less important with universal licensing.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int RegisteredAuthorsCreated { get; set; }

        /// <summary>
        /// Breaks out the Collaborators, may be less important with universal licensing.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int RegisteredCollaboratorsCreated { get; set; }

        /// <summary>
        /// Cumulative Authors created to date, anyone that every used an Author license or still has one.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int AuthorsCreatedtodate { get; set; }

        /// <summary>
        /// Cumulative Collaborators created to date, anyone that every used a Collaborator license or still has one
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int CollaboratorsCreatedtodate { get; set; }

        /// <summary>
        /// Authors who have actively used Blueprint or are currently part of an Author group 
        /// even if they’ve not accessed Blueprint
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int AuthorsActive { get; set; }

        /// <summary>
        /// Authors who have actively used Blueprint or are currently part of an Author group 
        /// and have accessed Blueprint at least once
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>

        public int AuthorsActiveLoggedOn { get; set; }

        /// <summary>
        /// Collaborators who have actively used Blueprint or are currently part 
        /// of an Collaborator group even if they’ve not accessed Blueprint
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int CollaboratorsActive { get; set; }

        /// <summary>
        /// Collaborators who have actively used Blueprint or are currently part of 
        /// an Collaborator group and have accessed Blueprint at least once
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int CollaboratorsActiveLoggedOn { get; set; }

    }
}
