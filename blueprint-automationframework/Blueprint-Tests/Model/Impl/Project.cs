using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;
using Logging;
using Model.Facades;
using System.Net;

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
            int projectId = 0;
            Dictionary<string, string> addedHeaders = CommonGetProjects(address, projectId, user);
            List<Project> projects = WebRequestFacade.CreateWebRequestAndGetResponse<List<Project>>(address + SVC_PROJECTS_PATH, "GET", addedHeaders);
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
            Dictionary<string, string> addedHeaders = CommonGetProjects(address, projectId, user);
            Project project = WebRequestFacade.CreateWebRequestAndGetResponse<Project>(address + SVC_PROJECTS_PATH + "/" + projectId, "GET", addedHeaders);
            return project;
        }

        /// <sumary>
        /// Internal CommonGetProjects method containing Logger and Header implementation based on passed parameters.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">(optional) The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with. Default to use no authentication. </param>
        /// <returns> a header containing valid authentication if the optional user parameter is passed.</returns>
        /// 
        private static Dictionary<string, string> CommonGetProjects(string address, int projectId = 0, IUser user = null)
        {
            Logger.WriteDebug((projectId == 0) ? "Creating HttpWebRequest for " + SVC_PROJECTS_PATH + "." : "Creating HttpWebRequest for /api/v1/projects/{0}.", projectId);
            Dictionary<string, string> addedHeaders = null;
            if (user != null)
            {
                BlueprintServer.GetUserToken(address, user);
                addedHeaders = BlueprintServer.GetTokenHeader(user);
            }
            return addedHeaders;
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
