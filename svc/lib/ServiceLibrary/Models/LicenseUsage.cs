using Newtonsoft.Json;

namespace ServiceLibrary.Models
{
    
    /// <summary>
    /// The structure to keep calculated license usage activities.
    /// </summary>
    /// <remarks>
    /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
    /// </remarks>
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
        /// Number of unique viewers who have accessed BP in a given month
        /// </summary>
        public int UniqueViewers { get; set; }

        /// <summary>
        /// Unique Viewers who have accessed Blueprint at least once.
        /// </summary>
        public int ViewersActiveLoggedOn { get; set; }

        /// <summary>
        /// Unique Authors who have  accessed Blueprint at least once
        /// </summary>
        public int AuthorsActiveLoggedOn { get; set; }

        /// <summary>
        /// Unique Collaborators who have have accessed Blueprint at least once.
        /// </summary>
        public int CollaboratorsActiveLoggedOn { get; set; }

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
        /// Breaks out the Viewers, may be less important with universal licensing.
        /// </summary>
        /// <remarks>
        /// Populated from LicenseActivities and LicenseActivitiesDetails tables.
        /// </remarks>
        public int RegisteredViewersCreated { get; set; }

        /// <summary>
        /// Combination of actual Author license activities and author license group participation. 
        /// </summary>
        /// <remarks>
        /// if a user accessed BP as an Author for the first time in June of 2015 I would count 
        /// them as a RegisteredAuthorCreated in June even if they are no longer assigned to an Author group
        /// </remarks>
        public int RegisteredAuthorsCreated { get; set; }

        /// <summary>
        /// Combination of actual Collaborator license activities and Collaborator license group participation.
        /// </summary>
        /// <remarks>
        /// if a user accessed BP as an Collaborator for the first time in June of 2015 I would count 
        /// them as a RegisteredCollaboratorsCreated in June even if they are no longer assigned to an Collaborator group
        /// </remarks>
        public int RegisteredCollaboratorsCreated { get; set; }

        /// <summary>
        /// Cumulative ​Authors created to date, anyone that ever used an Author license or still has one. 
        /// </summary>
        /// <remarks>
        /// Combination of looking at any license activity up to a given date and then the delta of users 
        /// who are in these license groups even if they haven’t generated any license activity.
        /// </remarks>
        public int AuthorsCreateToDate { get; set; }

        /// <summary>
        /// Cumulative Collaborators created to date, anyone that every used a Collaborator license or still has one
        /// </summary>
        /// <remarks>
        /// Combination of looking at any license activity up to a given date and then the delta of users 
        /// who are in these license groups even if they haven’t generated any license activity.
        /// </remarks>
        public int CollaboratorsCreatedToDate { get; set; }

        /// <summary>
        /// Unique Authors who have actively used Blueprint (logged on) or are currently part of an Author group 
        /// even if they’ve not accessed Blueprint. 
        /// </summary>
        public int AuthorsActive { get; set; }

        /// <summary>
        /// Unique Collaborators who have actively used Blueprint (logged on) or are currently part of 
        /// an Collaborator group even if they’ve not accessed Blueprint.
        /// </summary>
        public int CollaboratorsActive { get; set; }

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
