using Model.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IStoryteller
    {
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
        /// Creates a Process type artifact
        /// </summary>
        /// <param name="process">The artifact to be added.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">Expected status code for this call. By default, only '201 Success' is expected.</param>
        /// <returns></returns>
        IArtifact AddProcessArtifact(IArtifact process, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
        /// <summary>
        /// Deletes the process artifact
        /// </summary>
        /// <param name="process">The artifact to be deleted.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <returns></returns>
        IArtifactResultBase DeleteProcessArtifact(IArtifact process, IUser user);

        /// <summary>
        /// Gets list of processes for the specified projectId
        /// Runs /projects/id/processes
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="id">Id of the Project</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The list of processes</returns>
        IList<IProcess> GetProcesses(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Gets Id of the specified type from the specified project
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="projectId">Id of the Project</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The list of processes</returns>
        int GetProcessTypeId(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
