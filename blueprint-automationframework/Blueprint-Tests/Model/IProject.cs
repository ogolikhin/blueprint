using System.Collections.Generic;
using Model.Impl;
using System.Net;

namespace Model
{
    public interface IProject
    {
        #region Properties

        /// <summary>
        /// Id of the project
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Description of the project
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Full path for the project. e.g. /Blueprint/Project
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// Artifact type list for the project
        /// </summary>
        List<OpenApiArtifactType> ArtifactTypes { get; }

        /// <summary>
        /// Nova Artifact type list for the project.
        /// </summary>
        List<NovaArtifactType> NovaArtifactTypes { get; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a new project on the Blueprint server.
        /// </summary>
        void CreateProject();

        /// <summary>
        /// Deletes a project on the Blueprint server.
        /// </summary>
        void DeleteProject();

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        List<IProject> GetProjects(string address, IUser user = null);

        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with. Default to use no authentication. </param>
        /// <returns>a project associated with the projectId provided with the request.</returns>
        IProject GetProject(string address, int projectId, IUser user = null);

        /// <summary>
        /// Updates a project on the Blueprint server with the changes that were made to this object.
        /// </summary>
        void UpdateProject();

        /// <summary>
        /// Get the all artifactTypes for the project, update the ArtifactTypes of the project and return the same list
        /// Runs api/v1/projects/projectId/metadata/artifactTypes with optional parameter based on optional boolean parameter
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">The user to authenticate to the the server with.  Defaults to no authentication.</param>
        /// <param name="shouldRetrievePropertyTypes">(optional) Defines whether or not to include property types.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>a artifact type list which is retrieved for the project</returns>
        List<OpenApiArtifactType> GetAllArtifactTypes(string address, IUser user,
            bool shouldRetrievePropertyTypes = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Get the all NovaArtifactTypes for the project, update the ArtifactTypes of the project and return the same list
        /// Runs: GET {address}/svc/artifactstore/projects/{projectId}/meta/customtypes
        /// </summary>
        /// <param name="artifactStore">The ArtifactStore to use for the call.</param>
        /// <param name="user">The user to authenticate to the the server with.  Defaults to no authentication.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>a artifact type list which is retrieved for the project</returns>
        List<NovaArtifactType> GetAllNovaArtifactTypes(
            IArtifactStore artifactStore,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null);

        #endregion Methods
    }
}
