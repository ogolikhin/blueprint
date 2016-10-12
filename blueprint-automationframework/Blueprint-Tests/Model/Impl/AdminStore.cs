using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;
using Model.Factories;
using Model.ArtifactModel;

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
            RestApiFacade restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Sessions.SSO;

            string encodedSamlResponse = HashingUtilities.EncodeTo64UTF8(samlResponse);
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            Dictionary<string, string> queryParameters = null;

            if (force != null)
            {
                queryParameters = new Dictionary<string, string> { { "force", force.ToString() } };
            }

            Logger.WriteInfo("Adding SSO session for user '{0}'...", username);

            RestResponse response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                additionalHeaders,
                queryParameters,
                encodedSamlResponse,
                expectedStatusCodes);

            string token = GetToken(response);

            ISession session = new Session { UserName = username, IsSso = true, SessionId = token };
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
            ISession session = AddSession(user?.Username, user?.Password, force, expectedStatusCodes, expectedServiceErrorMessage);

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
            RestApiFacade restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.SESSIONS;

            string encodedUsername = HashingUtilities.EncodeTo64UTF8(username);
            string encodedPassword = (password != null) ? HashingUtilities.EncodeTo64UTF8(password) : null;
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "login", encodedUsername } };

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
                ISession session = new Session { UserName = username, IsSso = false, SessionId = token };
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
            RestApiFacade restApi = new RestApiFacade(Address);
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

        /// <seealso cref="IAdminStore.GetLoginUser(string, List{HttpStatusCode})"/>
        public IUser GetLoginUser(string token, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(Address, token);
            string path = RestPaths.Svc.AdminStore.Users.LOGINUSER;

            try
            {
                Logger.WriteInfo("Getting logged in user's info...");

                RestResponse response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: expectedStatusCodes);

                Logger.WriteInfo("Deserializing user object...");
                AdminStoreUser adminStoreUser = JsonConvert.DeserializeObject<AdminStoreUser>(response.Content);

                IUser user = UserFactory.CreateUserOnly();
                user.Department = adminStoreUser.Department;
                user.DisplayName = adminStoreUser.DisplayName;
                user.Email = adminStoreUser.Email;
                user.FirstName = adminStoreUser.FirstName;
                user.LastName = adminStoreUser.LastName;
                user.License = adminStoreUser.License;
                user.Username = adminStoreUser.Username;
                user.InstanceAdminRole = adminStoreUser.InstanceAdminRole;
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
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.AdminStore.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.AdminStore.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetSettings(IUser, List{HttpStatusCode})"/>
        public ConfigSettings GetSettings(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ISession session = SessionFactory.CreateSessionWithToken(user);
            return GetSettings(session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetSettings(ISession, List{HttpStatusCode})"/>
        public ConfigSettings GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Config.SETTINGS;

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting settings...");

            RestResponse response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            var settingsDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(response.Content);

            // We can't deserialize directly into a ConfigSettings object because AdminStore returns a dictionary inside a dictionary...
            ConfigSettings settings = new ConfigSettings(settingsDictionary);
            return settings;
        }

        /// <seealso cref="IAdminStore.GetConfigJs(IUser, List{HttpStatusCode})"/>
        public string GetConfigJs(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ISession session = SessionFactory.CreateSessionWithToken(user);
            return GetConfigJs(session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetConfigJs(ISession, List{HttpStatusCode})"/>
        public string GetConfigJs(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AdminStore.Config.CONFIG_JS;

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting config.js...");

            RestResponse response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return response.Content;
        }

        /// <seealso cref="IAdminStore.GetLicenseTransactions(int?, ISession, List{HttpStatusCode})"/>
        public IList<LicenseActivity> GetLicenseTransactions(int? numberOfDays, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(Address);
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

                RestResponse response = restApi.SendRequestAndGetResponse(
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
            ISession session = SessionFactory.CreateSessionWithToken(user);
            return GetLicenseTransactions(numberOfDays, session, expectedStatusCodes);
        }

        /// <seealso cref="IAdminStore.GetFolderById(int, IUser, List{HttpStatusCode})"/>
        public IPrimitiveFolder GetFolderById(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.FOLDERS_id_, id);
            string token = user?.Token?.AccessControlToken;

            RestResponse response = GetResponseFromRequest(path, id, token, expectedStatusCodes);

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

            RestResponse response = GetResponseFromRequest(path, id, token, expectedStatusCodes);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                primitiveFolderList = JsonConvert.DeserializeObject<List<PrimitiveFolder>>(response.Content);
                Assert.IsNotNull(primitiveFolderList, "Object could not be deserialized properly");
            }

            return primitiveFolderList;
        }

        /// <seealso cref="IAdminStore.GetProjectById(int, IUser, List{HttpStatusCode})"/>
        public IProject GetProjectById(int id, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AdminStore.Instance.PROJECTS_id_, id);
            string token = user?.Token?.AccessControlToken;

            RestResponse response = GetResponseFromRequest(path, id, token, expectedStatusCodes);

            IProject project = JsonConvert.DeserializeObject<InstanceProject>(response.Content);
            Assert.IsNotNull(project, "Object could not be deserialized properly");

            return project;
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
            RestApiFacade restApi = new RestApiFacade(Address, token);

            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "id", id.ToStringInvariant() } };
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

        /// <seealso cref="IAdminStore.ResetPassword(IUser, string, List{HttpStatusCode})"/>
        public void ResetPassword(IUser user, string newPassword, List<HttpStatusCode> expectedStatusCodes = null)
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

            Logger.WriteInfo("Resetting user '{0}' password from '{1}' to '{2}'", user.Username, user.Password, newPassword ?? "null");

            var restApi = new RestApiFacade(Address);
            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParameters,
                bodyObject: bodyObject,
                expectedStatusCodes: expectedStatusCodes);
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
