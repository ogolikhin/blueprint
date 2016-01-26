using System.Collections.Generic;
using System.Net;
using Model.Impl;

namespace Model
{
    public interface IAdminStore
    {
        List<ISession> Sessions { get; }

        /// <summary>
        /// Adds a new session in AdminStore for the specified user and returns the session object containing the new session token.
        /// (Runs: POST /sessions/{userId})
        /// </summary>
        /// <param name="username">(optional) The user name.</param>
        /// <param name="password">(optional) The user password.</param>
        /// <param name="force">(optional) Force new session creation if session for this user already exists</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <param name="expectedServiceErrorMessage">If an WebException is thrown, we will assert that it contains this expected error message.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSession(string username = null,
            string password = null,
            bool? force = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            IServiceErrorMessage expectedServiceErrorMessage = null);

        /// <summary>
        /// Adds a new session in AdminStore for the specified user and returns the session object containing the new session token.
        /// (Runs: POST /sessions/{userId})
        /// </summary>
        /// <param name="session">The session to add to AdminStore.</param>
        /// <param name="force">(optional) Force new session creation if session for this user already exists</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSession(ISession session, bool? force = null, List < HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified session from AdminStore.
        /// (Runs: DELETE /sessions)
        /// </summary>
        /// <param name="session">The session to delete.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a session for the specified user.
        /// (Runs: GET /sessions/{userId})
        /// </summary>
        /// <param name="userId">The ID of the user whose session you are getting.</param>
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
        /// Gets login user for specified token.
        /// (Runs: GET /users/loginuser)
        /// </summary>
        /// <param name="token">A token to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A user object.</returns>
        IUser GetLoginUser(string token, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the AdminStore service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by AdminStore.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        HttpStatusCode GetStatus(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets setting from ConfigControl .
        /// (Runs: GET /config/settings)
        /// </summary>
        /// <param name="session">A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Settings dictionary. Now it is empty.</returns>
        Dictionary<string, string> GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets config.js from ConfigControl.
        /// (Runs: GET /config/config.js)
        /// </summary>
        /// <param name="session">A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>config.js file.</returns>
        string GetConfigJs(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of license transactions.
        /// (Runs: GET /licenses/transactions?days=numberOfDays)
        /// </summary>
        /// <param name="numberOfDays">Period in days back from today.</param>
        /// <param name="session">(optional) A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of LicenseActivity.</returns>
        IList<LicenseActivity> GetLicenseTransactions(int numberOfDays, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
