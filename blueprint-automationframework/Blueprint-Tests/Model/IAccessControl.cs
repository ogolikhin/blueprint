using Model.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public enum LicenseState
    {
        active,
        locked
    }

    public interface IAccessControlLicensesInfo
    {
        int LicenseLevel { get; set; }
        int Count { get; set; }
    }

    public interface IAccessControl : IDisposable
    {
        /// <summary>
        /// Stores a list of sessions that were added to AccessControl, but haven't been deleted yet.
        /// Deleting a session should remove it from this list.
        /// </summary>
        List<ISession> Sessions { get; }

        /// <summary>
        /// Checks if the specified session is not expired and if the user is authorized to perform the specified operation on the specified artifact.
        /// If the session is valid, its timeout value is extended.
        /// (Runs: PUT /sessions?op={op}&aid={artifactId})
        /// </summary>
        /// <param name="session">A session containing a session token to check for authorization.</param>
        /// <param name="operation">(optional) The operation to authorize.</param>
        /// <param name="artifactId">(optional) The artifact to authorize.</param>
        /// <returns>The session that we just authorized.</returns>
        ISession AuthorizeOperation(ISession session, string operation = null, int? artifactId = null);

        /// <summary>
        /// Adds a new session in AccessControl for the specified user and returns the session object containing the new session token.
        /// (Runs: POST /sessions/{userId})
        /// </summary>
        /// <param name="userId">The User ID.</param>
        /// <param name="username">(optional) The user name.</param>
        /// <param name="beginTime">(optional) </param>
        /// <param name="endTime">(optional) </param>
        /// <param name="isSso">(optional) </param>
        /// <param name="licenseLevel">(optional) </param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSession(int userId,
            string username = null,
            DateTime? beginTime = null,
            DateTime? endTime = null,
            bool? isSso = null,
            int? licenseLevel = null,
            List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Adds a new session in AccessControl for the specified user and returns the session object containing the new session token.
        /// (Runs: POST /sessions/{userId})
        /// </summary>
        /// <param name="session">The session to add to AccessControl.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified session from AccessControl.
        /// (Runs: DELETE /sessions)
        /// </summary>
        /// <param name="session">The session to delete.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a session for the specified user.
        /// (Runs: GET /sessions/{userId})
        /// </summary>
        /// <param name="userId">The ID of the user whose session you want to get.</param>
        /// <returns>The session for the specified user.</returns>
        ISession GetSession(int? userId);

        /// <summary>
        /// Gets a session for the specified user.
        /// (Runs: GET /sessions/select?ps={ps}&pn={pn})
        /// </summary>
        /// <param name="adminToken">A token to identify an admin user.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="pageNumber">Page number.</param>
        /// <returns>A paged list of existing sessions.</returns>
        List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber);

        /// <summary>
        /// Checks if the AccessControl service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of all dependent services.</returns>
        string GetStatus(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the current status of the AccessControl service.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Status of File Store service.</returns>
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets number of active/locked licenses.
        /// (Runs: GET /licenses/active[locked])
        /// </summary>
        /// <param name="state">License state active or locked.</param>
        /// <param name="session">The AccessControl session.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>License level and number of licenses in this state.</returns>
        IList<IAccessControlLicensesInfo> GetLicensesInfo(LicenseState state, ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets number of active/locked licenses.
        /// (Runs: GET /licenses/active[locked])
        /// </summary>
        /// <param name="state">License state active or locked.</param>
        /// <param name="token">The AccessControl token.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>License level and number of licenses in this state.</returns>
        IList<IAccessControlLicensesInfo> GetLicensesInfo(LicenseState state, string token = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of license transactions.
        /// (Runs: GET /licenses/transactions?days=numberOfDays&consumerType=type)
        /// </summary>
        /// <param name="numberOfDays">Number of days of license transactions history.</param>
        /// <param name="consumerType">Application which created request. (ConsumerType - Client=1, Analytics=2).</param>
        /// <param name="session">(optional) A session to identify a user.</param>
        /// /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of LicenseActivity.</returns>
        IList<ILicenseActivity> GetLicenseTransactions(int numberOfDays, int consumerType, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets license usage information
        /// </summary>
        /// <param name="year">(optional)Information for specific year. By default gets information for all years</param>
        /// <param name="month">(optional)Information for specific month. By default gets information for all months</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>LicenseUsage.</returns>
        LicenseUsage GetLicenseUsage(int? year = null, int? month = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of active sessions.
        /// (Runs: GET /sessions/select?ps=PageSize&pn=PageNumber)
        /// </summary>
        /// <param name="pageSize">(optional) Number of sessions per page.</param>
        /// <param name="pageNumber">(optional) Number of page to display.</param>
        /// <param name="session">(optional) A session for authentication.</param>
        /// /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of sessions.</returns>
        IList<ISession> GetActiveSessions(int? pageSize = null, int? pageNumber = null, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
