using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
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

        #endregion Properties


        #region Methods

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
            if (user == null) { throw new ArgumentNullException("user"); }

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password);
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
            if (user == null) { throw new ArgumentNullException("user"); }

            RestApiFacade restApi = new RestApiFacade(address, user.Username, user.Password, user.Token.OpenApiToken);
            string path = string.Format("{0}/{1}", SVC_PROJECTS_PATH, projectId);
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
            return string.Format("[Project]: Id={0}, Name={1}, Description={2}, Location={3}", Id, Name, Description, Location);
        }

        #endregion Methods
    }
}
