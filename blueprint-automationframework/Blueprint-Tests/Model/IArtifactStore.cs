﻿using Model.Impl;
using System;
using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model
{
    /// <summary>
    /// This is the data returned by:  GET /projects/{projectId}/meta/customtypes  which contains all the artifact/sub-artifact & property types in a project.
    /// </summary>
    public class ProjectArtifactTypesResult
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
        /// Returns artifact, sub-artifact and property types of the specified project.
        /// (Runs: GET /projects/{projectId}/meta/customtypes)
        /// </summary>
        /// <param name="project">The project whose types you are interested in.</param>
        /// <param name="user">(optional) The user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A list of artifact, sub-artifact and property types.</returns>
        ProjectArtifactTypesResult GetArtifactTypes(IProject project, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

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

    }
}
