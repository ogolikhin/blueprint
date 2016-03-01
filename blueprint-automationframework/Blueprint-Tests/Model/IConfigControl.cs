using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IConfigControl
    {
        /// <summary>
        /// Gets the log file from ConfigControl (no authentication is required).
        /// </summary>
        /// <param name="user">(optional) The user credentials for the request.  No credentials should be required.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header.</param>
        /// <returns>The log file.</returns>
        IFile GetLog(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}
