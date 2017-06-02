using Model.Impl;
using Model.JobModel;
using Model.JobModel.Enums;
using Model.JobModel.Impl;
using System;
using System.Collections.Generic;
using System.Net;
using Model.Common.Enums;
using Model.NovaModel.AdminStoreModel;
using Utilities.Facades;

namespace Model
{
    public interface IAdminStore : IDisposable
    {
        /// <summary>
        /// Gets the URL address of the server.
        /// </summary>
        string Address { get; }

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
        /// Checks if the session for the specified user from AdminStore is valid.
        /// (Runs: GET /sessions/alive)
        /// </summary>
        /// <param name="user">The user that contains the session token to check.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The returned HTTP Status Code</returns>
        HttpStatusCode CheckSession(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        ///  Checks if the specified session from AdminStore is valid.
        /// (Runs: GET /sessions/alive)
        /// </summary>
        /// <param name="session">The session to check.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The returned HTTP Status Code</returns>
        HttpStatusCode CheckSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the specified session token from AdminStore is valid.
        /// (Runs: GET /sessions/alive)
        /// </summary>
        /// <param name="token">The session token to check.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The returned HTTP Status Code</returns>
        HttpStatusCode CheckSession(string token, List<HttpStatusCode> expectedStatusCodes = null);

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
        /// Adds a user.
        /// (Runs: POST /users)
        /// </summary>
        /// <param name="adminUser">The admin user adding the user.</param>
        /// <param name="user">An InstanceUser object representing the user to be added.</param>
        /// <returns>The id of the added user</returns>
        int AddUser(IUser adminUser, InstanceUser user);

        /// <summary>
        /// Deletes users.  To delete all users except those in the list, pass selectAll = true.
        /// (Runs: POST /users/delete)
        /// </summary>
        /// <param name="adminUser">The admin user deleting users.</param>
        /// <param name="ids">The ids to include or exclude, depending on selectAll value.</param>
        /// <param name="selectAll">(optional) The selection scope indicator. If false, 
        /// the users are included in the deletion. If true, the users are excluded from the deletion. 
        /// Default is false.</param>
        /// <returns>The DeleteResult object.</returns>
        DeleteResult DeleteUsers(IUser adminUser, List<int> ids, bool selectAll = false);

        /// <summary>
        /// Delete a single user.
        /// (Runs: POST /users/delete)
        /// </summary>
        /// <param name="adminUser">The admin user deleting the user.</param>
        /// <param name="id">The id to include or exclude, depending on selectAll value.</param>
        /// <param name="selectAll">(optional) The selection scope indicator. If false, 
        /// the user is included in the deletion. If true, the user is excluded from the deletion. 
        /// Default is false.</param>
        /// <returns>The DeleteResult object.</returns>
        DeleteResult DeleteUser(IUser adminUser, int id, bool selectAll = false);

        /// <summary>
        /// Gets login user for specified token.
        /// (Runs: GET /users/loginuser)
        /// </summary>
        /// <param name="token">A token to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A user object.</returns>
        IUser GetLoginUser(string token, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a user by user Id.
        /// (Runs: GET /users/{userId})
        /// </summary>
        /// <param name="adminUser">The admin user getting the user.</param>
        /// <param name="userId">The user id of the user.</param>
        /// <returns>An InstanceUser object</returns>
        InstanceUser GetUserById(IUser adminUser, int? userId);

        /// <summary>
        /// Gets a list of non-deleted users matching a specified filter.
        /// (Runs: GET /users?  with the following parameters: offset={integer}, limit={integer}, sort={string}, order={asc|desc}
        /// property1={value}..., propertyN={value}, search={string}
        /// </summary>
        /// <param name="adminUser">The admin user getting the users.</param>
        /// <param name="offset">(optional) 0-based index of the first item to return (Default: null).</param>
        /// <param name="limit">(optional) Maximum number of items to return (if any) (Default: null). 
        /// The server may return fewer items than requested (Default: null).</param>
        /// <param name="sort">(optional) Property name by which to sort results (Default: null).</param>
        /// <param name="order">(optional) "asc" sorts in ascending order; "desc" sorts in descending order (Default: null). 
        /// The default order depends on the particular property.</param>
        /// <param name="search">(optional) Search query that would be applied on predefined properties specific to data (Default: null).</param>
        /// <returns>A QueryResult object of InstanceUser objects</returns>
        QueryResult<InstanceUser> GetUsers(IUser adminUser,
            int? offset = null,
            int? limit = null,
            string sort = null,
            SortOrder? order = null,
            string search = null);

        /// <summary>
        /// Updates a user.
        /// (Runs: PUT /users/{userId})
        /// </summary>
        /// <param name="adminUser">The admin user updating the user.</param>
        /// <param name="user">An InstanceUser object representing the user to update.</param>
        /// <returns>The returned HTTP Status Code</returns>
        HttpStatusCode UpdateUser(IUser adminUser, InstanceUser user);

        /// <summary>
        /// Checks if the AdminStore service is ready for operation.
        /// (Runs: GET /status)
        /// </summary>
        /// <param name="preAuthorizedKey">(optional) The pre-authorized key to use for authentication.  Defaults to null.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>A JSON structure containing the status of this service and its dependent services.</returns>
        string GetStatus(string preAuthorizedKey = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Checks if the AdminStore service is ready for operation.
        /// (Runs: GET /status/upcheck)
        /// </summary>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The status code returned by AdminStore.</returns>
        HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets application settings from ConfigControl.
        /// (Runs: GET /config)
        /// </summary>
        /// <remarks>No authentication is required for this call.</remarks>
        /// <returns>A dictionary of application settings.</returns>
        Dictionary<string, string> GetApplicationSettings();

        /// <summary>
        /// Gets setting from ConfigControl.
        /// (Runs: GET /config/settings)
        /// </summary>
        /// <param name="user">The user containing the token to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing the config settings that were returned.</returns>
        ConfigSettings GetSettings(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets setting from ConfigControl.
        /// (Runs: GET /config/settings)
        /// </summary>
        /// <param name="session">A session to identify a user.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>An object containing the config settings that were returned.</returns>
        ConfigSettings GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null);

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
        /// Request the specified user's password to be reset.  This will cause an E-mail to be sent to the user with a HTTP link containing a password recovery token.
        /// (Runs: POST /svc/adminstore/users/passwordrecovery/request)
        /// No authentication is required, since the user is resetting their password because they forgot what it was...
        /// You can find the token that gets sent in the E-mail in the following table: Adminstore.dbo.PasswordRecoveryTokens
        /// DB Columns:  "Login" (i.e. the username), "CreationTime" (DateTime), and "RecoveryToken" (GUID string).
        /// </summary>
        /// <param name="username">The username whose password you want to reset.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The RestResponse.</returns>
        RestResponse PasswordRecoveryRequest(string username, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Reset the specified user's password to a new password.  Use the RequestPasswordRecovery() function first to get a recovery token.
        /// (Runs: POST /svc/adminstore/users/passwordrecovery/reset)
        /// No authentication is required, since the user is resetting their password because they forgot what it was...
        /// You can find the token that gets sent in the E-mail in the following table: Adminstore.dbo.PasswordRecoveryTokens
        /// DB Columns:  "Login" (i.e. the username), "CreationTime" (DateTime), and "RecoveryToken" (GUID string).
        /// </summary>
        /// <param name="recoveryToken">The RecoveryToken GUID string from the Adminstore.dbo.PasswordRecoveryTokens table.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        /// <returns>The RestResponse.</returns>
        RestResponse PasswordRecoveryReset(string recoveryToken, string newPassword, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Changes the user's password to a new one.
        /// (Runs: POST /svc/adminstore/users/reset?login={username})
        /// </summary>
        /// <param name="user">The user whose password you are resetting (should contain the old password).</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request.  By default only 200 OK is expected.</param>
        void ChangePassword(IUser user, string newPassword, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns HTTP code for REST request to get project.
        /// (Runs: GET instance/projects/projectId)
        /// </summary>
        /// <param name="projectId">An id of the specific project.</param>
        /// <param name="user">A user object.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>The requested InstanceProject.</returns>
        InstanceProject GetProjectById(int projectId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

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

        /// <summary>
        /// Returns NavigationPath for Project. 
        /// (Runs: GET svc/adminstore/instance/projects/projectId/navigationPath)
        /// </summary>
        /// <param name="projectId">An id of specific project.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="includeProjectItself">(optional) Should name of project be included. By default includes Project's name.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected.</param>
        /// <returns>Navigation Path.</returns>
        List<string> GetProjectNavigationPath(int projectId, IUser user, bool? includeProjectItself = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets the User Icon for the specified User ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose icon you want to retrieve.</param>
        /// <param name="user">A user to authenticate with.</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected,
        ///     but 204 No Content is also valid if the user has no icon.</param>
        /// <returns>The icon file.</returns>
        IFile GetCustomUserIcon(int userId, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get jobs available for user
        /// </summary>
        /// <param name="user">A user that authenticate with</param>
        /// <param name="page">(optional) page index of job results. The length of which is determined by the pageSize argument</param>
        /// <param name="pageSize">(optional) The maximum number of jobs on each page.</param>
        /// <param name="jobType">(optional) The job type that user can filter with. If null, returns all jobs without filtering</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected</param>
        /// <returns>JobResult</returns>
        JobResult GetJobs(IUser user, int? page=null, int? pageSize=null, JobType? jobType=null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Get a job for the user
        /// </summary>
        /// <param name="user">A user that authenticate with</param>
        /// <param name="jobId">The job ID</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 200 OK is expected</param>
        /// <returns>JobInfo</returns>
        IJobInfo GetJob(IUser user, int jobId, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a result file from the job result
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="jobId">The job ID</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <returns>The file that was requested</returns>
        IFile GetJobResultFile(IUser user, int jobId, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Schedules a job for test generation from the provided processes
        /// </summary>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="processTestJobParametersRequest">parameter form required for adding process test generation job</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request. By default only 201 Created is expected</param>
        /// <returns>AddJobResult</returns>
        AddJobResult QueueGenerateProcessTestsJob(IUser user, GenerateProcessTestsJobParameters processTestJobParametersRequest, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Gets a list of Instance Admin Roles
        /// </summary>
        /// <param name="adminUser">The admin user getting the instance roles list.</param>
        /// <returns>The retrieved list of Instance Admin Roles</returns>
        List<AdminRole> GetInstanceRoles(IUser adminUser);
    }
}
