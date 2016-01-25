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
        /// <param name="Id">Id of the Process artifact</param>
        /// <param name="versionIndex">(optional) Version of the process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <returns>The requested process artifact</returns>
        IProcess GetProcess(IUser user, int Id, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Updates a Process artifact
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="Process">The updated Process artifact</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        void UpdateProcess(IUser user, Process Process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
