using System.Collections.Generic;
using Model.OpenApiModel;

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
        /// Gets the id of the specified artifact type from the specified project.
        /// Runs api/v1/projects/projectId/metadata/artifactTypes
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">Id of the project</param>
        /// <param name="baseArtifactTypeName">Name of the base artifact type (Actor, Process, Storyboard)</param>
        /// <param name="user">(optional) The user to authenticate to the the server with.  Defaults to no authentication.</param>
        /// <returns>Id of the specified artifact type from the specified project.</returns>
        int GetArtifactTypeId(string address, int projectId, BaseArtifactType baseArtifactTypeName, IUser user = null);

        #endregion Methods
    }
}
