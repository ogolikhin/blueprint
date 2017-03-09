using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;

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
        /// Instance of ArtifactStore associated with the project
        /// </summary>
        IArtifactStore ArtifactStore { get; set; }

        /// <summary>
        /// Artifact type list for the project
        /// </summary>
        List<OpenApiArtifactType> ArtifactTypes { get; }

        /// <summary>
        /// Nova Artifact type list for the project.
        /// </summary>
        List<NovaArtifactType> NovaArtifactTypes { get; }

        /// <summary>
        /// Nova Property type list for the project.
        /// </summary>
        List<NovaPropertyType> NovaPropertyTypes { get; }

        /// <summary>
        /// Nova sub-Artifact type list for the project.
        /// </summary>
        List<NovaArtifactType> NovaSubArtifactTypes { get; }

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
        /// Gets the default Baseline and Review folder for this project.
        /// </summary>
        /// <param name="address">The base Uri address of the ArtifactStore server.</param>
        /// <param name="user">The user to authenticate to the server with.</param>
        /// <returns>The default Baseline and Review for this project.</returns>
        INovaArtifactBase GetDefaultBaselineFolder(IUser user);

        /// <summary>
        /// Gets the default Collection folder for this project.
        /// </summary>
        /// <param name="address">The base Uri address of the ArtifactStore server.</param>
        /// <param name="user">The user to authenticate to the server with.</param>
        /// <returns>The default Collection folder for this project.</returns>
        INovaArtifactBase GetDefaultCollectionFolder(IUser user);

        /// <summary>
        /// Converts the specified Predefined (Base) Type into the specific Item Type ID for this project.
        /// </summary>
        /// <param name="predefinedType">The base predefined type to convert.</param>
        /// <returns>The Item Type Id of the predefined type for this project.</returns>
        int GetItemTypeIdForPredefinedType(ItemTypePredefined predefinedType);

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
        /// <returns>a artifact type list which is retrieved for the project</returns>
        List<OpenApiArtifactType> GetAllOpenApiArtifactTypes(string address, IUser user,
            bool shouldRetrievePropertyTypes = false, List<HttpStatusCode> expectedStatusCodes = null);

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

        /// <summary>
        /// Returns itemTypeId for the base artifact type of the current project
        /// </summary>
        /// <param name="itemTypePredefined">itemTypePredefined for which itemTypeId should be returned</param>
        /// <returns>itemTypeId</returns>
        /// <exception cref="ArgumentNullException">Throws when NovaArtifactTypes is empty</exception>
        /// <exception cref="AssertionException">Throws when itemTypePredefined cannot be found within NovaArtifactTypes</exception>
        int GetNovaBaseItemTypeId(ItemTypePredefined itemTypePredefined);

        #endregion Methods
    }
}
