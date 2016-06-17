using Model.Impl;
using Model.ArtifactModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
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
        /// Gets all children artifacts for specified by id project.
        /// (Runs: GET /projects/{projectId}/children)
        /// </summary>
        /// <param name="id">The id of specified project.</param>
        /// <param name="user">Current user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Response content</returns>
        List<ArtifactType> GetProjectChildrenByProjectId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets all children artifacts by project and artifact id.
        /// (Runs: GET /projects/{projectId}/artifacts/{artifactId})
        /// </summary>
        /// <param name="projectId">The id of specific project.</param>
        /// <param name="artifactId">The id of specific artifact.</param>
        /// <param name="user">Current user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Response content.</returns>
        List<ArtifactType> GetArtifactChildrenByProjectAndArtifactId(int projectId, int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets artifacts history by artifact id.
        /// (Runs: GET /svc/ArtifactStore/artifacts/{artifactId}/version)
        /// </summary>
        /// <param name="artifactId">The id of artifact.</param>
        /// <param name="user">Current user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of artifacts versions.</returns>
        List<ArtifactHistoryVersion> GetArtifactHistory(int artifactId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

    }
}
