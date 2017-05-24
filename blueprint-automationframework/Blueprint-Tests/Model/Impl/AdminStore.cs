using Common;
using Model.ArtifactModel;
using Model.Factories;
using Model.JobModel;
using Model.JobModel.Enums;
using Model.JobModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp.Extensions.MonoHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Model.Common.Enums;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class AdminStore : NovaServiceBase, IAdminStore
    {
        private const string TOKEN_HEADER = BlueprintToken.ACCESS_CONTROL_TOKEN_HEADER;

        public List<IArtifact> Artifacts { get; } = new List<IArtifact>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the AdminStore service.</param>
        public AdminStore(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        /// <summary>
        /// Extracts the session token from the specified RestResponse.
        /// </summary>
        /// <param name="response">The RestResponse containing the session token.</param>
        /// <returns>A session token, or null if no token was found.</returns>
        private static string GetToken(RestResponse response)
        {
            string token = null;

            if (response.Headers.ContainsKey(TOKEN_HEADER))
            {
                token = (string)response.Headers[TOKEN_HEADER];
            }

            return token;
        }

        #region Members inherited from IAdminStore

        /// <seealso cref="IAdminStore.Sessions"/>
        public List<ISession> Sessions { get; } = new List<ISession>();

        /// <seealso cref="IAdminStore.AddSsoSession(string, string, bool?, List{HttpStatusCode})"/>
        public ISession AddSsoSession(string username, string samlResponse, bool? force = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Sessions.SSO;

            string encodedSamlResponse = HashingUtilities.EncodeTo64UTF8(samlResponse);
            var additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            Dictionary<string, string> queryParameters = null;

            if (force != null)
            {
                queryParameters = new Dictionary<string, string> { { "force", force.ToString() } };
            }

            Logger.WriteInfo("Adding SSO session for user '{0}'...", username);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                additionalHeaders,
                queryParameters,
                encodedSamlResponse,
                expectedStatusCodes);

            string token = GetToken(response);

            var session = new Session { UserName = username, IsSso = true, SessionId = token };
            Logger.WriteDebug("Got session token '{0}' for User: {1}.", token, username);

            // Add session to list of created sessions, so we can delete them later.
            Sessions.Add(session);
            Logger.WriteDebug("Content = '{0}'", restApi.Content);

            return session;
        }

        /// <seealso cref="IAdminStore.AddSession(IUser, bool?, List{HttpStatusCode}, IServiceErrorMessage)"/>
        public ISession AddSession(IUser user = null,
            bool? force = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            IServiceErrorMessage expectedServiceErrorMessage = null)
        {
            var session = AddSession(user?.Username, user?.Password, force, expectedStatusCodes, expectedServiceErrorMessage);

            if (user != null)
            {
                user.SetToken(session.SessionId);
            }

            return session;
        }

        /// <seealso cref="IAdminStore.AddSession(string, string, bool?, List{HttpStatusCode}, IServiceErrorMessage)"/>
        public ISession AddSession(string username = null, string password = null, bool? force = null,
            List<HttpStatusCode> expectedStatusCodes = null, IServiceErrorMessage expectedServiceErrorMessage = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.SESSIONS;

            string encodedUsername = HashingUtilities.EncodeTo64UTF8(username);
            string encodedPassword = (password != null) ? HashingUtilities.EncodeTo64UTF8(password) : null;
            var additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            var queryParameters = new Dictionary<string, string> { { "login", encodedUsername } };

            if (force != null)
            {
                queryParameters.Add("force", force.ToString());
            }
            
            try
            {
                Logger.WriteInfo("Adding session for user '{0}'...", username);

                RestResponse response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.POST,
                    additionalHeaders,
                    queryParameters,
                    encodedPassword,
                    expectedStatusCodes);

                string token = GetToken(response);
                //similar to code in SessionController.cs from AdminStore
                var session = new Session { UserName = username, IsSso = false, SessionId = token };
                Logger.WriteDebug("Got session token '{0}' for User: {1}.", token, username);

                // Add session to list of created sessions, so we can delete them later.
                Sessions.Add(session);
                Logger.WriteDebug("Content = '{0}'", restApi.Content);

                return session;
            }
            catch (WebException)
            {
                Logger.WriteDebug("Content = '{0}'", restApi.Content);

                if (expectedServiceErrorMessage != null)
                {
                    var serviceErrorMessage = JsonConvert.DeserializeObject<ServiceErrorMessage>(restApi.Content);
                    serviceErrorMessage.AssertEquals(expectedServiceErrorMessage);
                }

                throw;
            }
        }

        /// <seealso cref="IAdminStore.CheckSession(IUser, List{HttpStatusCode})"/>
        public HttpStatusCode CheckSession(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return CheckSession(user?.Token?.AccessControlToken, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.CheckSession(ISession, List{HttpStatusCode})"/>
        public HttpStatusCode CheckSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return CheckSession(session?.SessionId, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.CheckSession(string, List{HttpStatusCode})"/>
        public HttpStatusCode CheckSession(string token = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address, token);
            const string path = RestPaths.Svc.AdminStore.Sessions.ALIVE;

            Logger.WriteInfo("Checking session '{0}'...", token);

            var restResponse = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return restResponse.StatusCode;
        }

        /// <seealso cref="IAdminStore.DeleteSession(IUser, List{HttpStatusCode})"/>
        public void DeleteSession(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            DeleteSession(user?.Token?.AccessControlToken, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.DeleteSession(ISession, List{HttpStatusCode})"/>
        public void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            DeleteSession(session?.SessionId, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.DeleteSession(string, List{HttpStatusCode})"/>
        public void DeleteSession(string token, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.SESSIONS;

            Dictionary<string, string> additionalHeaders = null;

            if (token != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, token } };
            }

            Logger.WriteInfo("Deleting session '{0}'...", token);

            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.DELETE,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            // Remove token from the list of created sessions.
            Sessions.RemoveAll(session => session.SessionId == token);
        }

        /// <seealso cref="IAdminStore.AddUser(IUser, InstanceUser)"/>
        public int AddUser(IUser adminUser, InstanceUser user)
        {
            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            string path = RestPaths.Svc.AdminStore.Users.USERS;

            if (user != null && user.Password != null)
            {
                user.Password = HashingUtilities.EncodeTo64UTF8(user.Password);
            }

            try
            {
                Logger.WriteInfo("Creating user...");

                var response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.POST,
                    bodyObject: user,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.Created });

                return I18NHelper.Int32ParseInvariant(response.Content);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing AddUser - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.DeleteUsers(IUser, List{int?}, bool)"/>
        public HttpStatusCode DeleteUsers(IUser adminUser, List<int?> ids,  bool selectAll = false)
        {
            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            const string path = RestPaths.Svc.AdminStore.Users.USERS_DELETE;

            try
            {
                Logger.WriteInfo("Deleting users with selectAll: {0} and ids: ({1})", selectAll, string.Join(", ", ids));

                var response = restApi.SendRequestAndGetResponse
                (
                    path,
                    RestRequestMethod.POST,
                    bodyObject: new { SelectAll = selectAll, Ids = ids }
                );

                return response.StatusCode;
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing DeleteUsers - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.GetLoginUser(string, List{HttpStatusCode})"/>
        public IUser GetLoginUser(string token, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address, token);
            string path = RestPaths.Svc.AdminStore.Users.LOGINUSER;

            try
            {
                Logger.WriteInfo("Getting logged in user's info...");

                var loginUser = restApi.SendRequestAndDeserializeObject<LoginUser>(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: expectedStatusCodes);

                var user = UserFactory.CreateUserOnly();
                user.DisplayName = loginUser.DisplayName;
                user.Email = loginUser.Email;
                user.FirstName = loginUser.FirstName;
                user.LastName = loginUser.LastName;
                user.License = loginUser.LicenseType;
                user.Username = loginUser.Login;
                user.InstanceAdminRole = loginUser.InstanceAdminRoleId;
                user.SetToken(token);

                return user;
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting GetLoginUser - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.GetUserById(IUser, int?)"/>
        public InstanceUser GetUserById(IUser adminUser, int? userId)
        {
            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Users.USERS_id_, userId);

            try
            {
                Logger.WriteInfo("Getting user by user id...");

                return restApi.SendRequestAndDeserializeObject<InstanceUser>(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.OK },
                    shouldControlJsonChanges: false);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing GetUserById - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.GetUsers(IUser, int?, int?, string, SortOrder?, string)"/>
        public QueryResult<InstanceUser> GetUsers(IUser adminUser, 
            int? offset = null, 
            int? limit = null, 
            string sort = null, 
            SortOrder? order = null,
            string search = null)
        {
            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Users.USERS);

            var queryParameters = new Dictionary<string, string>();

            if (offset != null)
            {
                queryParameters.Add("offset", offset.ToStringInvariant());
            }

            if (limit != null)
            {
                queryParameters.Add("limit", limit.ToStringInvariant());
            }

            if (!string.IsNullOrEmpty(sort))
            {
                queryParameters.Add("sort", sort);
            }
            
            if (order != null)
            {
                queryParameters.Add("order", order.ToString());
            }

            if (!string.IsNullOrEmpty(search))
            {
                queryParameters.Add("search", search);
            }

            try
            {
                Logger.WriteInfo("Getting users...");

                return restApi.SendRequestAndDeserializeObject<QueryResult<InstanceUser>>(
                    path,
                    RestRequestMethod.GET,
                    queryParameters: queryParameters,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.OK },
                    shouldControlJsonChanges: false);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing GetUsers - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.UpdateUser(IUser, InstanceUser)"/>
        public HttpStatusCode UpdateUser(IUser adminUser, InstanceUser user)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Users.USERS_id_, user.Id);

            try
            {
                Logger.WriteInfo("Updating user with Id: {0}", user.Id);

                var response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.PUT,
                    bodyObject: user,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.OK });

                return response.StatusCode;
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing UpdateUser - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.GetSession(int?)"/>
        public ISession GetSession(int? userId)
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IAdminStore.GetSession(string, uint, uint)"/>
        public List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber)
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IAdminStore.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.AdminStore.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.AdminStore.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetApplicationSettings()"/>
        public Dictionary<string, string> GetApplicationSettings()
        {
            var restApi = new RestApiFacade(Address);
            const string path = RestPaths.Svc.AdminStore.CONFIG;

            Logger.WriteInfo("Getting application settings...");

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET);

            var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content);
            return settings;
        }

        /// <seealso cref="IAdminStore.GetSettings(IUser, List{HttpStatusCode})"/>
        public ConfigSettings GetSettings(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var session = SessionFactory.CreateSessionWithToken(user);
            return GetSettings(session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetSettings(ISession, List{HttpStatusCode})"/>
        public ConfigSettings GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Config.SETTINGS;

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting settings...");

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(response.Content);

            // We can't deserialize directly into a ConfigSettings object because AdminStore returns a dictionary inside a dictionary...
            var settings = new ConfigSettings(settingsDictionary);
            return settings;
        }

        /// <seealso cref="IAdminStore.GetConfigJs(IUser, List{HttpStatusCode})"/>
        public string GetConfigJs(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var session = SessionFactory.CreateSessionWithToken(user);
            return GetConfigJs(session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetConfigJs(ISession, List{HttpStatusCode})"/>
        public string GetConfigJs(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Config.CONFIG_JS;

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting config.js...");

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return response.Content;
        }

        /// <seealso cref="IAdminStore.GetLicenseTransactions(int?, ISession, List{HttpStatusCode})"/>
        public IList<LicenseActivity> GetLicenseTransactions(int? numberOfDays, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Licenses.TRANSACTIONS;

            Dictionary<string, string> additionalHeaders = null;
            Dictionary<string, string> queryParameters = null;

            if (numberOfDays != null)
            {
                Logger.WriteDebug("Adding 'days={0}' as a query parameter.", numberOfDays);
                queryParameters = new Dictionary<string, string> { { "days", numberOfDays.Value.ToStringInvariant() } };
            }

            if (session != null)
            {
                Logger.WriteDebug("Adding Token header:  {0} = {1}", TOKEN_HEADER, session.SessionId);
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            try
            {
                Logger.WriteInfo("Getting list of License Transactions...");

                var response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.GET,
                    additionalHeaders: additionalHeaders,
                    queryParameters: queryParameters,
                    expectedStatusCodes: expectedStatusCodes);

                return JsonConvert.DeserializeObject<List<LicenseActivity>>(response.Content);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting list of License Transactions - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.GetLicenseTransactions(IUser, int?, List{HttpStatusCode})"/>
        public IList<LicenseActivity> GetLicenseTransactions(IUser user, int? numberOfDays, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var session = SessionFactory.CreateSessionWithToken(user);
            return GetLicenseTransactions(numberOfDays, session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetFolderById(int, IUser, List{HttpStatusCode})"/>
        public IPrimitiveFolder GetFolderById(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.FOLDERS_id_, id);
            string token = user?.Token?.AccessControlToken;

            var response = GetResponseFromRequest(path, id, token, expectedStatusCodes);

            var primitiveFolder = JsonConvert.DeserializeObject<PrimitiveFolder>(response.Content);
            Assert.IsNotNull(primitiveFolder, "Object could not be deserialized properly");

            return primitiveFolder;
        }

        /// <seealso cref="IAdminStore.GetFolderChildrenByFolderId(int, IUser, List{HttpStatusCode})"/>
        public List<PrimitiveFolder> GetFolderChildrenByFolderId(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.Folders_id_.CHILDREN, id);
            List<PrimitiveFolder> primitiveFolderList = null;
            string token = user?.Token?.AccessControlToken;

            var response = GetResponseFromRequest(path, id, token, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                primitiveFolderList = JsonConvert.DeserializeObject<List<PrimitiveFolder>>(response.Content);
                Assert.IsNotNull(primitiveFolderList, "Object could not be deserialized properly");
            }

            return primitiveFolderList;
        }

        /// <seealso cref="IAdminStore.GetProjectById(int, IUser, List{HttpStatusCode})"/>
        public InstanceProject GetProjectById(int projectId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.PROJECTS_id_, projectId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var returnedInstanceProject = restApi.SendRequestAndDeserializeObject<InstanceProject>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true
                );

            return returnedInstanceProject;
        }

        /// <seealso cref="IAdminStore.GetCustomUserIcon(int, IUser, List{HttpStatusCode})"/>
        public IFile GetCustomUserIcon(int userId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            IFile file = null;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Users_id_.ICON, userId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    FileType = response.ContentType
                };
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Assert.IsEmpty(response.Content, "Response body contains data, even though Status Code was 204 No Content!");
            }

            return file;
        }

        /// <summary>
        /// Executes the REST request and returns the response.
        /// </summary>
        /// <param name="path">The REST path.</param>
        /// <param name="id">An 'id' to add as a query parameter.</param>
        /// <param name="token">The token for the request.</param>
        /// <param name="expectedStatusCodes">The expected status codes.</param>
        /// <returns>The RestResponse from the request.</returns>
        private RestResponse GetResponseFromRequest(string path, int id, string token, List<HttpStatusCode> expectedStatusCodes)
        {
            var restApi = new RestApiFacade(Address, token);

            var queryParameters = new Dictionary<string, string> { { "id", id.ToStringInvariant() } };
            Dictionary<string, string> additionalHeaders = null;

            try
            {
                Logger.WriteInfo("Getting artifact - {0}", id);

                return restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.GET,
                    additionalHeaders: additionalHeaders,
                    queryParameters: queryParameters,
                    expectedStatusCodes: expectedStatusCodes);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting response - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.PasswordRecoveryRequest(string, List{HttpStatusCode})"/>
        public RestResponse PasswordRecoveryRequest(string username, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var path = RestPaths.Svc.AdminStore.Users.PasswordRecovery.REQUEST;
            string bodyObject = username;

            Logger.WriteInfo("Requesting password reset for user '{0}'.", username ?? "null");

            var restApi = new RestApiFacade(Address);
            return restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                bodyObject: bodyObject,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.PasswordRecoveryReset(string, string, List{HttpStatusCode})"/>
        public RestResponse PasswordRecoveryReset(string recoveryToken,
            string newPassword,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var path = RestPaths.Svc.AdminStore.Users.PasswordRecovery.RESET;
            var bodyObject = new Dictionary<string, string>();

            if (recoveryToken != null)
            {
                bodyObject.Add("Token", recoveryToken);
            }

            if (newPassword != null)
            {
                bodyObject.Add("Password", newPassword.EncodeToBase64());
            }

            Logger.WriteInfo("Resetting password for token '{0}' to '{1}'.", recoveryToken, newPassword);

            var restApi = new RestApiFacade(Address);
            return restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                bodyObject: bodyObject,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.ChangePassword(IUser, string, List{HttpStatusCode})"/>
        public void ChangePassword(IUser user, string newPassword, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var path = RestPaths.Svc.AdminStore.Users.RESET;
            var bodyObject = new Dictionary<string, string>();

            if (user.Password != null)
            {
                string encodedOldPassword = HashingUtilities.EncodeTo64UTF8(user.Password);
                bodyObject.Add("OldPass", encodedOldPassword);
            }

            if (newPassword != null)
            {
                string encodedNewPassword = HashingUtilities.EncodeTo64UTF8(newPassword);
                bodyObject.Add("NewPass", encodedNewPassword);
            }

            var queryParameters = new Dictionary<string, string> { { "login", HashingUtilities.EncodeTo64UTF8(user.Username) } };

            Logger.WriteInfo("Changing user '{0}' password from '{1}' to '{2}'", user.Username, user.Password, newPassword ?? "null");

            var restApi = new RestApiFacade(Address);
            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParameters,
                bodyObject: bodyObject,
                expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetProjectNavigationPath(int, IUser, bool?, List{HttpStatusCode})"/>
        public List<string> GetProjectNavigationPath(int projectId, IUser user, bool? includeProjectItself = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.Projects_id_.NAVIGATIONPATH,
                projectId);
            

            var queryParameters = new Dictionary<string, string>();

            if (includeProjectItself != null)
            {
                queryParameters.Add("includeProjectItself", includeProjectItself.ToString());
            }

            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            var navigationPath = restApi.SendRequestAndDeserializeObject<List<string>>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return navigationPath;
        }

        /// <seealso cref="IAdminStore.GetJobs(IUser, int?, int?, JobType?, List{HttpStatusCode})"/>
        public JobResult GetJobs(IUser user, int? page=null, int? pageSize=null, JobType? jobType=null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(AdminStore), nameof(GetJobs));

            var path = RestPaths.Svc.AdminStore.JOBS;

            var queryParams = new Dictionary<string, string>();

            if (page != null)
            {
                queryParams.Add("page", page.ToString());
            }

            if (pageSize != null)
            {
                queryParams.Add("pageSize", pageSize.ToString());
            }

            if (jobType != null)
            {
                queryParams.Add("jobType", jobType.ToString());
            }

            var tokenValue = user?.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            var returnedJobResult = restApi.SendRequestAndDeserializeObject<JobResult>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParams,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return returnedJobResult;
        }

        /// <seealso cref="IAdminStore.GetJob(IUser, int, List{HttpStatusCode})"/>
        public IJobInfo GetJob(IUser user, int jobId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(AdminStore), nameof(GetJob));

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.JOBS_id_, jobId);

            var tokenValue = user?.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            var jobInfo = restApi.SendRequestAndDeserializeObject<JobInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return jobInfo;
        }

        /// <seealso cref="IAdminStore.GetJobResultFile(IUser, int, List{HttpStatusCode})"/>
        public IFile GetJobResultFile(IUser user, int jobId, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(AdminStore), nameof(GetJobResultFile));

            File file = null;

            var path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Jobs_id_.Result.FILE, jobId);

            var tokenValue = user?.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            // Create a file represents output file associated with the job
            // TODO: Guid for the file cannot be retrieved with any existing API calls at this point and there is no plan to get the guid.
            // TODO: Will introduce the sql calls later if it's required
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string filename = HttpUtility.UrlDecode(new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName);

                file = new File
                {
                    Content = response.RawBytes.ToArray(),
                    FileType = response.ContentType,
                    FileName = filename
                };
            }

            return file;
        }

        /// <seealso cref="IAdminStore.QueueGenerateProcessTestsJob(IUser, GenerateProcessTestsJobParameters, List{HttpStatusCode})"/>
        public AddJobResult QueueGenerateProcessTestsJob(IUser user, GenerateProcessTestsJobParameters processTestJobParametersRequest, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(AdminStore), nameof(QueueGenerateProcessTestsJob));

            var tokenValue = user?.Token?.AccessControlToken;

            var restApi = new RestApiFacade(Address, tokenValue);

            // Set expectedStatusCodes to 201 Created by default if it's null.
            expectedStatusCodes = expectedStatusCodes ?? new List<HttpStatusCode> { HttpStatusCode.Created };

            var addJobResult = restApi.SendRequestAndDeserializeObject<AddJobResult, GenerateProcessTestsJobParameters>(
                RestPaths.Svc.AdminStore.Jobs.Process.TESTGEN,
                RestRequestMethod.POST,
                processTestJobParametersRequest,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: true);

            return addJobResult;
        }

        public List<AdminRole> GetInstanceRoles(IUser adminUser)
        {
            var restApi = new RestApiFacade(Address, adminUser?.Token?.AccessControlToken);
            string path = RestPaths.Svc.AdminStore.Users.INSTANCE_ROLES;

            try
            {
                Logger.WriteInfo("Getting instance roles...");

                return restApi.SendRequestAndDeserializeObject<List<AdminRole>>(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: new List<HttpStatusCode> { HttpStatusCode.OK},
                    shouldControlJsonChanges: true);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while performing GetInstanceRoles - {0}", ex.Message);
                throw;
            }
        }

        #endregion Members inherited from IAdminStore

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all sessions that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(AdminStore), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Delete all the sessions that were created.
                foreach (var session in Sessions.ToArray())
                {
                    // AdminStore removes and adds a new session in some cases, so we should expect a 401 error in some cases.
                    List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK, HttpStatusCode.Unauthorized };
                    DeleteSession(session, expectedStatusCodes);
                }

                Sessions.Clear();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all sessions that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
