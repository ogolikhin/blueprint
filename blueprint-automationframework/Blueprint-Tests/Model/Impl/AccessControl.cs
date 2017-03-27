using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Utilities;
using Utilities.Facades;
using Newtonsoft.Json;

namespace Model.Impl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class AccessControl : NovaServiceBase, IAccessControl
    {
        private const string TOKEN_HEADER = BlueprintToken.ACCESS_CONTROL_TOKEN_HEADER;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the AccessControl service.</param>
        public AccessControl(string address)
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

        #region Members inherited from IAccessControl

        /// <seealso cref="IAccessControl.Sessions"/>
        public List<ISession> Sessions { get; } = new List<ISession>();

        /// <seealso cref="IAccessControl.AuthorizeOperation(ISession, string, int?)"/>
        public ISession AuthorizeOperation(ISession session, string operation = null, int? artifactId = null)    // PUT /sessions?op={op}&aid={artifactId}
        {
            ThrowIf.ArgumentNull(session, nameof(session));

            var restApi = new RestApiFacade(Address, session.SessionId);
            string path = RestPaths.Svc.AccessControl.SESSIONS;
            Dictionary<string, string> queryParameters = null;

            if ((operation != null) || artifactId.HasValue)
            {
                queryParameters = new Dictionary<string, string>();

                if (operation != null) { queryParameters.Add("op", operation); }

                if (artifactId.HasValue) { queryParameters.Add("aid", artifactId.ToString());}
            }

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Put Session for User ID: {0}.", session.UserId);

            var returnedSession = restApi.SendRequestAndDeserializeObject<Session>(
                path,
                RestRequestMethod.PUT,
                queryParameters: queryParameters);

            return returnedSession;
        }

        /// <seealso cref="IAccessControl.AddSession(int, string, DateTime?, DateTime?, bool?, int?, List{HttpStatusCode})"/>
        public ISession AddSession(int userId,
            string username = null,
            DateTime? beginTime = null,
            DateTime? endTime = null,
            bool? isSso = null,
            int? licenseLevel = null,
            List<HttpStatusCode> expectedStatusCodes = null)     // POST /sessions/{userId}
        {
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AccessControl.SESSIONS_id_, userId);
            var queryParameters = new Dictionary<string, string>();

            // Add all the query parameters for the POST call.
            queryParameters.Add("UserId", userId.ToStringInvariant());

            if (username != null) { queryParameters.Add("UserName", username); }

            if (beginTime != null) { queryParameters.Add("BeginTime", beginTime.Value.ToStringInvariant("o")); }

            if (endTime != null) { queryParameters.Add("EndTime", endTime.Value.ToStringInvariant("o")); }

            if (isSso != null) { queryParameters.Add("IsSso", isSso.Value.ToString());}

            if (licenseLevel != null) { queryParameters.Add("LicenseLevel", licenseLevel.Value.ToStringInvariant());}

            // Execute POST and get session token from the response.
            Logger.WriteInfo("Creating session for User ID: {0}.", userId);
            var restApi = new RestApiFacade(Address);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes);

            string token = GetToken(response);
            var session = new Session(userId, username, isSso.GetValueOrDefault(), licenseLevel.GetValueOrDefault(), token, beginTime, endTime);
            Logger.WriteDebug("Got session token '{0}' for User ID: {1}.", token, userId);

            // Add session to list of created sessions, so we can delete them later.
            Sessions.Add(session);

            return session;
        }

        /// <seealso cref="IAccessControl.AddSession(ISession, List{HttpStatusCode})"/>
        public ISession AddSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)   // POST /sessions/{userId}
        {
            ThrowIf.ArgumentNull(session, nameof(session));

            return AddSession(session.UserId,
                session.UserName,
                session.BeginTime,
                session.EndTime,
                session.IsSso,
                session.LicenseLevel,
                expectedStatusCodes);
        }

        /// <seealso cref="IAccessControl.DeleteSession(ISession, List{HttpStatusCode})"/>
        public void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)  // DELETE /sessions
        {
            var restApi = new RestApiFacade(Address, session?.SessionId);
            string path = RestPaths.Svc.AccessControl.SESSIONS;

            Logger.WriteInfo("Deleting session '{0}'.", session?.SessionId);

            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);

            // Remove the session from the list of created sessions.
            Sessions.Remove(session);
        }

        /// <seealso cref="IAccessControl.GetSession(int?)"/>
        public ISession GetSession(int? userId)    // GET /sessions/{userId}
        {
            var restApi = new RestApiFacade(Address);
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.AccessControl.SESSIONS_id_,
                (userId.HasValue ? userId.Value.ToStringInvariant() : string.Empty));

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Getting Session for User ID: {0}.", userId);

            var session = restApi.SendRequestAndDeserializeObject<Session>(path, RestRequestMethod.GET);

            return session;
        }

        /// <seealso cref="IAccessControl.GetSession(string, uint, uint)"/>
        public List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber) // GET /sessions/select?ps={ps}&pn={pn}
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IAccessControl.GetStatus(List{HttpStatusCode})"/>
        public string GetStatus(List<HttpStatusCode> expectedStatusCodes = null)    // GET /status
        {
            return GetStatus(RestPaths.Svc.AccessControl.STATUS, preAuthorizedKey: null, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IAccessControl.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.AccessControl.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IAccessControl.GetLicensesInfo(LicenseState, ISession, List{HttpStatusCode})"/>
        public IList<IAccessControlLicensesInfo> GetLicensesInfo(LicenseState state, ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetLicensesInfo(state, session?.SessionId, expectedStatusCodes);

        }

        /// <seealso cref="IAccessControl.GetLicensesInfo(LicenseState, string, List{HttpStatusCode})"/>
        public IList<IAccessControlLicensesInfo> GetLicensesInfo(LicenseState state, string token = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            string path;
            Dictionary<string, string> additionalHeaders = null;

            if (token != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, token } };
            }

            switch (state)
            {
                case LicenseState.active:
                    path = RestPaths.Svc.AccessControl.Licenses.ACTIVE;
                    break;
                case LicenseState.locked:
                    path = RestPaths.Svc.AccessControl.Licenses.LOCKED;
                    break;
                default:
                    throw new ArgumentException("state must be in LicenseState enum");
            }

            Logger.WriteInfo("Getting license information...");
            var restApi = new RestApiFacade(Address);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            switch (state)
            {
                case LicenseState.active:
                    var licenseInfoList = JsonConvert.DeserializeObject<List<AccessControlLicensesInfo>>(response.Content);
                    return licenseInfoList.ConvertAll(o => (IAccessControlLicensesInfo)o);
                case LicenseState.locked:
                    var licenseInfo = JsonConvert.DeserializeObject<AccessControlLicensesInfo>(response.Content);
                    return new List<IAccessControlLicensesInfo> { licenseInfo };
                default:
                    throw new ArgumentException("state must be in LicenseState enum");
            }
        }

        /// <seealso cref="IAccessControl.GetLicenseTransactions(int, int, ISession, List{HttpStatusCode})"/>
        public IList<ILicenseActivity> GetLicenseTransactions(int numberOfDays, int consumerType, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AccessControl.Licenses.TRANSACTIONS;

            var queryParameters = new Dictionary<string, string> { { "days", numberOfDays.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
            queryParameters.Add("consumerType", consumerType.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
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

                var licenseTransactions = JsonConvert.DeserializeObject<List<LicenseActivity>>(response.Content);
                return licenseTransactions.ConvertAll(o => (ILicenseActivity)o);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting list of License Transactions - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAccessControl.GetLicenseUsage(int?, int?, List{HttpStatusCode})"/>
        public LicenseUsage GetLicenseUsage(int? year = null, int? month = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(Address);
            string path = RestPaths.Svc.AccessControl.Licenses.USAGE;

            var queryParameters = new Dictionary<string, string>();

            if (month != null)
            {
                queryParameters.Add("month", month.ToString());
            }

            if (year != null)
            {
                queryParameters.Add("year", year.ToString());
            }

            try
            {
                Logger.WriteInfo("Getting list of used licenses...");

                var licenseUsageInfos = restApi.SendRequestAndDeserializeObject<LicenseUsage>(
                    path,
                    RestRequestMethod.GET,
                    queryParameters: queryParameters,
                    expectedStatusCodes: expectedStatusCodes,
                    shouldControlJsonChanges: true);

                return licenseUsageInfos;
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting list of license usage information - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAccessControl.GetActiveSessions(int?, int?, ISession, List{HttpStatusCode})"/>
        public IList<ISession> GetActiveSessions(int? pageSize = null, int? pageNumber = null, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var queryParameters = new Dictionary<string, string>();

            if (pageSize != null)
            {
                queryParameters.Add("ps", pageSize.ToString());
            }

            if (pageNumber != null)
            {
                queryParameters.Add("pn", pageNumber.ToString());
            }

            var restApi = new RestApiFacade(Address);
            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            try
            {
                Logger.WriteInfo("Getting list of active sessions...");

                if (queryParameters.Count == 0)
                {
                    queryParameters = null;
                }

                string path = RestPaths.Svc.AccessControl.Sessions.SELECT;

                var response = restApi.SendRequestAndGetResponse(
                    path,
                    RestRequestMethod.GET,
                    additionalHeaders: additionalHeaders,
                    queryParameters: queryParameters,
                    expectedStatusCodes: expectedStatusCodes);

                var sessions = JsonConvert.DeserializeObject<List<Session>>(response.Content);
                return sessions.ConvertAll(o => (ISession)o);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting list of active sessions - {0}", ex.Message);
                throw;
            }
        }
        #endregion Members inherited from IAccessControl

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all sessions that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(AccessControl), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Delete all the sessions that were created.
                foreach (var session in Sessions.ToArray())
                {
                    DeleteSession(session);
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
