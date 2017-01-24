
using Newtonsoft.Json;

namespace Model.Impl
{
    // Copied from: blueprint/svc/lib/ServiceLibrary/Models/LicenseUsage.cs
    public class LicenseUsage
    {
        #region Properties

        /// <summary>
        /// Year.
        /// </summary>
        public int UsageYear { get; set; }

        /// <summary>
        /// Month.
        /// </summary>
        public int UsageMonth { get; set; }

        /// <summary>
        /// Number of unique authors who have accessed BP in a given month
        /// </summary>
        public int UniqueAuthors { get; set; }

        /// <summary>
        /// Number of unique collaborators who have accessed BP in a given month
        /// </summary>
        public int UniqueCollaborators { get; set; }

        /// <summary>
        /// List of unique user ids who has author license and accessed BP
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string UniqueAuthorUserIds { get; set; }

        /// <summary>
        /// List of unique user ids who has collaborator license and accessed BP
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string UniqueCollaboratorUserIds { get; set; }

        /// <summary>		
        /// Combination of actual Author license activities and author license group participation. 		
        /// </summary>		
        /// <remarks>		
        /// if a user accessed BP as an Author for the first time in June of 2015 I would count 		
        /// them as a RegisteredAuthorCreated in June even if they are no longer assigned to an Author group		
        /// </remarks>		
        public int RegisteredAuthorsCreated { get; set; }

        /// <summary>
        /// List of user ids who has actual Author license (active)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string RegisteredAuthorsCreatedUserIds { get; set; }

        /// <summary>		
        /// Combination of actual Collaborator license activities and Collaborator license group participation.		
        /// </summary>		
        /// <remarks>		
        /// if a user accessed BP as an Collaborator for the first time in June of 2015 I would count 		
        /// them as a RegisteredCollaboratorsCreated in June even if they are no longer assigned to an Collaborator group		
        /// </remarks>		
        public int RegisteredCollaboratorsCreated { get; set; }

        /// <summary>
        /// List of user ids who has actual Collaborator license (active)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string RegisteredCollaboratorCreatedUserIds { get; set; }

        /// <summary>		
        /// Cumulative ​Authors created to date, anyone that ever used an Author license or still has one. 		
        /// </summary>		
        /// <remarks>		
        /// Combination of looking at any license activity up to a given date and then the delta of users 
        /// who are in these license groups even if they haven’t generated any license activity.	
        /// </remarks>		
        public int AuthorsCreatedToDate { get; set; }

        /// <summary>		
        /// umulative Collaborators created to date, anyone that ever used a Collaborator license or still has one.		
        /// </summary>		
        /// <remarks>		
        /// Combination of looking at any license activity up to a given date and then the delta of users 
        /// who are in these license groups even if they haven’t generated any license activity.		
        /// </remarks>		
        public int CollaboratorsCreatedToDate { get; set; }

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
