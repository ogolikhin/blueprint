using System;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Common;
using Utilities;
using Utilities.Facades;


namespace Model.Impl
{
    [DataContract(Name = "Project", Namespace = "Model")]
    public class Project : IProject
    {
        #region Properties

        /// <summary>
        /// PATHs of the project related APIs
        /// </summary>
        private const string SVC_PROJECTS_PATH = "/api/v1/projects";
        private const string URL_ARTIFACTTYPES = "metadata/artifactTypes";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";


        /// <summary>
        /// Id of the project
        /// </summary>
        [JsonProperty("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the project
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Full path for the project. e.g. /Blueprint/Project
        /// </summary>
        public string Location { get; set; }

        [SuppressMessage("Microsoft.Usage","CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ArtifactType>>))]
        public List<ArtifactType> ArtifactTypes { get; set; }

        #endregion Properties

        #region Constructors

        public Project()
        {
            ArtifactTypes = new List<ArtifactType>();
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Creates a new project on the Blueprint server.
        /// </summary>
        public void CreateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a project on the Blueprint server.
        /// </summary>
        public void DeleteProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        public List<IProject> GetProjects(string address, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Username, user?.Password, user?.Token?.OpenApiToken);
            List<Project> projects = restApi.SendRequestAndDeserializeObject<List<Project>>(SVC_PROJECTS_PATH, RestRequestMethod.GET);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }

        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with. Default to use no authentication. </param>
        /// <returns>a project associated with the projectId provided with the request.</returns>
        public IProject GetProject(string address, int projectId, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Username, user?.Password, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant("{0}/{1}", SVC_PROJECTS_PATH, projectId);
            Project project = restApi.SendRequestAndDeserializeObject<Project>(path, RestRequestMethod.GET);

            return project;
        }

        /// <summary>
        /// Updates a project on the Blueprint server with the changes that were made to this object.
        /// </summary>
        public void UpdateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return I18NHelper.FormatInvariant("[Project]: Id={0}, Name={1}, Description={2}, Location={3}", Id, Name, Description, Location);
        }

        public void GetAllArtifactTypes(
            string address,
            IUser user,
            bool isPropertyTypesRetrieveRequired = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, tokenValue);

            var path = isPropertyTypesRetrieveRequired ? I18NHelper.FormatInvariant("{0}/{1}/{2}?PropertyTypes=true", SVC_PROJECTS_PATH, Id, URL_ARTIFACTTYPES)
                : I18NHelper.FormatInvariant("{0}/{1}/{2}", SVC_PROJECTS_PATH, Id, URL_ARTIFACTTYPES);

            // Retrieve the artifact type list for the project 
            var artifactTypes = restApi.SendRequestAndDeserializeObject<List<ArtifactType>>(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            // Clean and repopulate ArtifactTypes if there is any element exist for ArtifactTypes
            if (ArtifactTypes.Any())
            {
                ArtifactTypes.Clear();

                foreach (var artifactType in artifactTypes)
                { 
                    ArtifactTypes.Add(artifactType);   
                }
            }

            ArtifactTypes = artifactTypes;
        }

        #endregion Public Methods

    }
}
