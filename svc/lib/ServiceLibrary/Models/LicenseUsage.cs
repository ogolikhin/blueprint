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
        /// Maxumumn number of concurent Viewer licenses.
        /// </summary>
        public int MaxConcurrentViewers { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Author licenses.
        /// </summary>
        public int MaxConcurrentAuthors { get; set; }

        /// <summary>
        /// Maxumumn number of concurent Collaborator licenses.
        /// </summary>
        public int MaxConcurrentCollaborators { get; set; }

        /// <summary>		
        /// Breaks out the Viewers, may be less important with universal licensing.		
        /// </summary>		
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
        /// Number of users logged in from Analytics.
        /// </summary>
        public int UsersFromAnalytics { get; set; }

        /// <summary>
        /// Number of users logged in from RestAPI.
        /// </summary>
        public int UsersFromRestApi { get; set; }


    }
}
