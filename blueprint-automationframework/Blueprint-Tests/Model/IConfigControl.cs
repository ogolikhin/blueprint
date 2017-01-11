using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IConfigControl : IDisposable
    {
        /// <summary>
        /// Gets the log file from ConfigControl (no authentication is required).
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.</param>
        /// <returns>The log file.</returns>
        IFile GetLog(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the ConfigControl service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of all dependent services.</returns>
        string GetStatus(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the ConfigControl service.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Status of File Store service.</returns>
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);
    }
}
