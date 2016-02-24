using Model.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IStoryteller
    {
        /// <summary>
        /// List of created artifacts.
        /// </summary>
        List<IOpenApiArtifact> Artifacts { get; }

        /// <summary>
        /// Creates a Process type artifact
        /// </summary>
        /// <param name="project">The project where the Process artifact is to be added</param>
        /// <param name="artifactType">The base artifact type of the Process artifact</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status code for this call. By default, only '201 Success' is expected.</param>
        /// <returns></returns>
        IOpenApiArtifact CreateProcessArtifact(IProject project, BaseArtifactType artifactType, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Creates multiple Process artifacts
        /// </summary>
        /// <param name="project">The project where the Process artifact is to be added</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="numberOfArtifacts"></param>
        /// <returns></returns>
        List<IOpenApiArtifact> CreateProcessArtifacts(IProject project, IUser user, int numberOfArtifacts);

        /// <summary>
        /// Gets a Process artifact
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="id">Id of the Process artifact</param>
        /// <param name="versionIndex">(optional) Version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The requested process artifact</returns>
        IProcess GetProcess(IUser user, int id, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets a Process artifact from a breadcrumb trail
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="ids">Ids in the breadcrumb trail.  The last Id is the Id of the Process being retrieved.</param>
        /// <param name="versionIndex">(optional) Version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The requested process artifact</returns>
        IProcess GetProcessWithBreadcrumb(IUser user, List<int> ids, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets list of processes for the specified projectId
        /// Runs /projects/id/processes
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="projectId">Id of the Project</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The list of processes</returns>
        IList<IProcess> GetProcesses(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets Id of the process artifact type for the specified project
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="project">specified project</param>
        /// <returns>The id of process artifact type</returns>
        int GetProcessTypeId(IUser user, IProject project);

        /// <summary>
        /// Updates a Process artifact
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="process">The updated Process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        void UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish Process artifact(s)
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="isKeepLock">(optional) Boolean parameter which define the weather or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The list of PublishArtifactResult after the call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<PublishArtifactResult> PublishProcessArtifacts(IUser user, bool isKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Deletes the process artifact
        /// </summary>
        /// <param name="artifact">The artifact to be deleted.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns></returns>
        IArtifactResult<IOpenApiArtifact> DeleteProcessArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Generates UserStorie artifact(s) from the target process artifact.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="processArtifact">The processArtifact to generate UserStoryArtifacts.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>The list of OpenApiUserStoryArtifact that generated or updated after the call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<OpenApiUserStoryArtifact> GenerateUserStories(IUser user, IOpenApiArtifact processArtifact, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
