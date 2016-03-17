using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IConfigControl
    {
        /// <summary>
        /// Gets the log file from ConfigControl (no authentication is required).
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The log file.</returns>
        IFile GetLog(List<HttpStatusCode> expectedStatusCodes = null);
    }
}
