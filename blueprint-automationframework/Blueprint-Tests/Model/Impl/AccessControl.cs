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
    public class AccessControl : IAccessControl
    {
        private const string SVC_PATH = "accesscontrol";
        private const string TOKEN_HEADER = BlueprintToken.ACCESS_CONTROL_TOKEN_HEADER;

        private static string _address;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the AccessControl service.</param>
        public AccessControl(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
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

        public List<ISession> Sessions { get; } = new List<ISession>();

        public ISession AuthorizeOperation(ISession session, string operation = null, int? artifactId = null)    // PUT /sessions?op={op}&aid={artifactId}
        {
            ThrowIf.ArgumentNull(session, nameof(session));

            var restApi = new RestApiFacade(_address, session.SessionId);
            string path = I18NHelper.FormatInvariant("{0}/sessions", SVC_PATH);
            Dictionary<string, string> queryParameters = null;

            if ((operation != null) || artifactId.HasValue)
            {
                queryParameters = new Dictionary<string, string>();

                if (operation != null) { queryParameters.Add("op", operation); }

                if (artifactId.HasValue) { queryParameters.Add("aid", artifactId.ToString());}
            }

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Put Session for User ID: {0}.", session.UserId);
            ISession returnedSession = restApi.SendRequestAndDeserializeObject<Session>(path, RestRequestMethod.PUT, queryParameters: queryParameters);

            return returnedSession;
        }

        public ISession AddSession(int userId,
            string username = null,
            DateTime? beginTime = null,
            DateTime? endTime = null,
            bool? isSso = null,
            int? licenseLevel = null,
            List<HttpStatusCode> expectedStatusCodes = null)     // POST /sessions/{userId}
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/sessions/{1}", SVC_PATH, userId);
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();

            // Add all the query parameters for the POST call.
            queryParameters.Add("UserId", userId.ToStringInvariant());

            if (username != null) { queryParameters.Add("UserName", username); }

            if (beginTime != null) { queryParameters.Add("BeginTime", beginTime.Value.ToStringInvariant("o")); }

            if (endTime != null) { queryParameters.Add("EndTime", endTime.Value.ToStringInvariant("o")); }

            if (isSso != null) { queryParameters.Add("IsSso", isSso.Value.ToString());}

            if (licenseLevel != null) { queryParameters.Add("LicenseLevel", licenseLevel.Value.ToStringInvariant());}

            // Execute POST and get session token from the response.
            Logger.WriteInfo("Creating session for User ID: {0}.", userId);
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);

            string token = GetToken(response);
            ISession session = new Session(userId, username, licenseLevel.GetValueOrDefault(), isSso.GetValueOrDefault(), token, beginTime, endTime);
            Logger.WriteDebug("Got session token '{0}' for User ID: {1}.", token, userId);

            // Add session to list of created sessions, so we can delete them later.
            Sessions.Add(session);

            return session;
        }

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

        public void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)  // DELETE /sessions
        {
            var restApi = new RestApiFacade(_address, session?.SessionId);
            string path = I18NHelper.FormatInvariant("{0}/sessions", SVC_PATH);

            Logger.WriteInfo("Deleting session '{0}'.", session?.SessionId);
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE, expectedStatusCodes: expectedStatusCodes);

            // Remove the session from the list of created sessions.
            Sessions.Remove(session);
        }

        public ISession GetSession(int? userId)    // GET /sessions/{userId}
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/sessions/{1}", SVC_PATH, (userId.HasValue ? userId.Value.ToStringInvariant() : string.Empty));

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Getting Session for User ID: {0}.", userId);
            ISession session = restApi.SendRequestAndDeserializeObject<Session>(path, RestRequestMethod.GET);
            return session;
        }

        public List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber) // GET /sessions/select?ps={ps}&pn={pn}
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode GetStatus(List<HttpStatusCode> expectedStatusCodes = null)    // GET /status
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/status", SVC_PATH);

            Logger.WriteInfo("Getting AccessControl status...");
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
            return response.StatusCode;
        }

        public List<ACLicensesInfo> GetLicensesInfo(LicenseState state, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path;
            switch (state)
            {
                case LicenseState.active:
                    {
                        path = I18NHelper.FormatInvariant("{0}/licenses/active", SVC_PATH);
                        break;
                    }
                case LicenseState.locked:
                    {
                        path = I18NHelper.FormatInvariant("{0}/licenses/locked", SVC_PATH);
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("state must be in LicenseState enum");
                    }
            }
            try
            {
                Logger.WriteInfo("Getting license information...");
                var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
                return JsonConvert.DeserializeObject<List<ACLicensesInfo>>(response.Content);
            }
            catch (WebException ex)
            {
                Logger.WriteDebug("Unexpected exception while trying to get license information - {0}", ex.Message);
                throw;
            }
        }

        public List<LicenseActivity> GetLicenseTransactions(int numberOfDays, int consumerType, ISession session = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/licenses/transactions", SVC_PATH);

            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "days", numberOfDays.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
            queryParameters.Add("consumerType", consumerType.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Dictionary<string, string> additionalHeaders = null;
            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }
            try
            {
                Logger.WriteInfo("Getting list of License Transactions...");
                RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, additionalHeaders: additionalHeaders,
                queryParameters: queryParameters);
                return JsonConvert.DeserializeObject<List<LicenseActivity>>(response.Content);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting GetLoginUser - {0}", ex.Message);
                throw;
            }
        }
        #endregion Members inherited from IAccessControl
    }
}
