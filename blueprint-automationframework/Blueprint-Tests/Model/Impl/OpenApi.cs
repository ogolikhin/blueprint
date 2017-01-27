using System.Collections.Generic;
using Common;
using Utilities.Facades;

namespace Model.Impl
{
    public static class OpenApi
    {
        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Default to use no authentication.</param>
        /// <returns>A project associated with the projectId provided with the request.</returns>
        public static IProject GetProject(string address, int projectId, IUser user = null)
        {
            var restApi = new RestApiFacade(address, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.PROJECTS_id_, projectId);
            var project = restApi.SendRequestAndDeserializeObject<Project>(path, RestRequestMethod.GET);

            return project;
        }

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        public static List<IProject> GetProjects(string address, IUser user = null)
        {
            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string path = RestPaths.OpenApi.PROJECTS;

            var projects = restApi.SendRequestAndDeserializeObject<List<Project>>(path, RestRequestMethod.GET);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }


    }
}
