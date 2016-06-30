﻿using Model.Impl;
using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{
    public interface IAdminStore : IDisposable
    {
        List<ISession> Sessions { get; }

        /// <summary>
        /// Adds a new session in AdminStore for the specified user and returns the session object containing the new session token.
        /// The new token is also added to the IUser object that was passed in.
        /// (Runs: POST /sessions?login={encrypted username}  or  POST /sessions?login={encrypted username}&amp;force=True)
        /// </summary>
        /// <param name="user">(optional) The user to authenticate.</param>
        /// <param name="force">(optional) Force new session creation if session for this user already exists</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <param name="expectedServiceErrorMessage">If an WebException is thrown, we will assert that it contains this expected error message.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSession(IUser user = null,
            bool? force = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            IServiceErrorMessage expectedServiceErrorMessage = null);

        /// <summary>
        /// Adds a new session in AdminStore for the specified user and returns the session object containing the new session token.
        /// (Runs: POST /sessions?login={encrypted username}  or  POST /sessions?login={encrypted username}&amp;force=True)
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
        /// Adds a new SSO session in AdminStore using the SAML response from an Identity Provider and returns the session object containing the new session token.
        /// (Runs: POST /sessions/sso?force=True)
        /// </summary>
        /// <param name="username">The username in the samlResponse.</param>
        /// <param name="samlResponse">The SAML response XML (unencrypted) to send to AdminStore.</param>
        /// <param name="force">(optional) Force new session creation if session for this user already exists</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A session object containing the new session token.</returns>
        ISession AddSsoSession(string username, string samlResponse, bool? force = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the session token for the specified user from AdminStore.
        /// (Runs: DELETE /sessions)
        /// </summary>
        /// <param name="user">The user that contains the session token to delete.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void DeleteSession(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified session from AdminStore.
        /// (Runs: DELETE /sessions)
        /// </summary>
        /// <param name="session">The session to delete.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Deletes the specified session token from AdminStore.
        /// (Runs: DELETE /sessions)
        /// </summary>
        /// <param name="token">The session token to delete.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void DeleteSession(string token, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a session for the specified user.
        /// (Runs: GET /sessions/{userId})
        /// </summary>
        /// <param name="userId">The ID of the user whose session you are getting.</param>
        /// <returns>The session for the specified user.</returns>
        ISession GetSession(int? userId);

        /// <summary>
        /// Gets a session for the specified user.
        /// (Runs: GET /sessions/select?ps={ps}&amp;pn={pn})
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
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to a valid key.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the AdminStore service is ready for operation.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by AdminStore.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets setting from ConfigControl.
        /// (Runs: GET /config/settings)
        /// </summary>
        /// <param name="user">The user containing the token to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Settings dictionary. Now it is empty.</returns>
        Dictionary<string, object> GetSettings(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets setting from ConfigControl.
        /// (Runs: GET /config/settings)
        /// </summary>
        /// <param name="session">A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>Settings dictionary. Now it is empty.</returns>
        Dictionary<string, object> GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets config.js from ConfigControl.
        /// (Runs: GET /config/config.js)
        /// </summary>
        /// <param name="user">The user containing the token to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>config.js file.</returns>
        string GetConfigJs(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

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
        /// <param name="numberOfDays">Number of days of license transactions history.  Passing null will omit the 'days' parameter from the GET request.</param>
        /// <param name="session">(optional) A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of LicenseActivity.</returns>
        IList<LicenseActivity> GetLicenseTransactions(int? numberOfDays, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets list of license transactions.
        /// (Runs: GET /licenses/transactions?days=numberOfDays)
        /// </summary>
        /// <param name="user">The user containing the token to authenticate with.</param>
        /// <param name="numberOfDays">Number of days of license transactions history.  Passing null will omit the 'days' parameter from the GET request.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>List of LicenseActivity.</returns>
        IList<LicenseActivity> GetLicenseTransactions(IUser user, int? numberOfDays, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Reset the user's password with a new one.
        /// (Runs: POST /users/reset?login={username})
        /// </summary>
        /// <param name="user">The user whose password you are resetting (should contain the old password).</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void ResetPassword(IUser user, string newPassword, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns HTTP code for REST request to get project.
        /// (Runs: GET instance/projects/projectId)
        /// </summary>
        /// <param name="id">An id of specific project.</param>
        /// <param name="user">A user object.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>Response content.</returns>
        IProject GetProjectById(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns list of aerifacts for REST request to get folder. 
        /// (Runs: GET instance/folders/folderId/children)
        /// </summary>
        /// <param name="id">An id of specific folder.</param>
        /// <param name="user">A user object.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>Response content.</returns>
        List<PrimitiveFolder> GetFolderChildrenByFolderId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns PrimitiveFolder for REST request to get folder. 
        /// (Runs: GET instance/folders/folderId)
        /// </summary>
        /// <param name="id">An id of specific folder.</param>
        /// <param name="user">A user object.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>Response content.</returns>
        IPrimitiveFolder GetFolderById(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
