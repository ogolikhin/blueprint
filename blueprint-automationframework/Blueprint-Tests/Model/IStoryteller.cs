using Model.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IStoryteller
    {
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
        /// Updates a Process artifact
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="process">The updated Process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        void UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
        
        /// <summary>
        /// Deletes the process artifact
        /// </summary>
        /// <param name="process">The artifact to be deleted.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns></returns>
        IArtifactResult<IOpenApiArtifact> DeleteProcessArtifact(IOpenApiArtifact process, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
