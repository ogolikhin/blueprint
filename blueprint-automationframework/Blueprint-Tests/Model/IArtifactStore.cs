using Model.Impl;
using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    /// <summary>
    /// This is the data returned by:  GET /projects/{projectId}/meta/customtypes  which contains all the custom artifact/sub-artifact and property types in a project.
    /// </summary>
    public class ProjectCustomArtifactTypesResult
    {
        public List<NovaArtifactType> ArtifactTypes { get; } = new List<NovaArtifactType>();
        public List<NovaArtifactType> SubArtifactTypes { get; } = new List<NovaArtifactType>();
        public List<NovaPropertyType> PropertyTypes { get; } = new List<NovaPropertyType>();
    }

    public interface IArtifactStore : IDisposable
    {
        /// <summary>
        /// Checks if the ArtifactStore service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the ArtifactStore service is ready for operation.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by ArtifactStore.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns all custom artifact, sub-artifact and property types of the specified project.
        /// (Runs: GET /projects/{projectId}/meta/customtypes)
        /// </summary>
        /// <param name="project">The project whose types you are interested in.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing a list of custom artifact, sub-artifact and property types for the specified project.</returns>
        ProjectCustomArtifactTypesResult GetCustomArtifactTypes(IProject project, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets all children artifacts for specified by id project.
        /// (Runs: GET /projects/{projectId}/children)
        /// </summary>
        /// <param name="id">The id of specified project.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of all artifacts in the specified project.</returns>
        List<Artifact> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets all children artifacts by project and artifact id.
        /// (Runs: GET /projects/{projectId}/artifacts/{artifactId})
        /// </summary>
        /// <param name="projectId">The id of specific project.</param>
        /// <param name="artifactId">The id of specific artifact.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of all sub-artifacts of the specified artifact.</returns>
        List<Artifact> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifacts history by artifact id.
        /// (Runs: GET /svc/ArtifactStore/artifacts/{artifactId}/version)
        /// </summary>
        /// <param name="artifactId">The id of artifact.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="sortByDateAsc">(optional) False - the latest version comes first, true the latest version comes last. Without this param call return versions as for sortByDateAsc=false.</param>
        /// <param name="limit">(optional) The maximum number of history items returned in the request. Without this param call return 10 versions.</param>
        /// <param name="offset">(optional) The offset for the pagination.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of artifacts versions.</returns>
        List<ArtifactHistoryVersion> GetArtifactHistory(int artifactId, IUser user,
            bool? sortByDateAsc = null, int? limit = null, int? offset = null,
            List<HttpStatusCode> expectedStatusCodes = null);

    }
}
