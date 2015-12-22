using System;
using System.Collections.Generic;
using System.Net;
using Logging;
using Utilities.Facades;


namespace Model.Impl
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces")]
    public class AccessControl : IAccessControl
    {
        private const string SVC_PATH = "svc/accesscontrol";
        private const string TOKEN_HEADER = "Session-Token";

        private static string _address;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The URI address of the AccessControl service.</param>
        public AccessControl(string address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

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

        public ISession AuthorizeOperation(ISession session) // PUT /sessions
        {
            throw new NotImplementedException();
        }

        public ISession AuthorizeOperation(ISession session, string operation) // PUT /sessions/{op}
        {
            throw new NotImplementedException();
        }

        public ISession AuthorizeOperation(ISession session, string operation, int artifactId)    // PUT /sessions/{op}/{artifactId}
        {
            if (session == null) { throw new ArgumentNullException(nameof(session)); }
            if (operation == null) { throw new ArgumentNullException(nameof(operation)); }

            var restApi = new RestApiFacade(_address, session.SessionId);
            string path = string.Format("{0}/sessions/{1}/{2}", SVC_PATH, operation, artifactId);
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string> {{ TOKEN_HEADER, session.SessionId }};

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Put Session for User ID: {0}.", session.UserId);
            ISession returnedSession = restApi.SendRequestAndDeserializeObject<Session>(path, RestRequestMethod.PUT, additionalHeaders);

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
            string path = string.Format("{0}/sessions/{1}", SVC_PATH, userId);
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();

            // Add all the query parameters for the POST call.
            queryParameters.Add("UserId", userId.ToString());

            if (username != null) { queryParameters.Add("UserName", username); }

            if (beginTime != null) { queryParameters.Add("BeginTime", beginTime.Value.ToString("o")); }

            if (endTime != null) { queryParameters.Add("EndTime", endTime.Value.ToString("o")); }

            if (isSso != null) { queryParameters.Add("IsSso", isSso.Value.ToString());}

            if (licenseLevel != null) { queryParameters.Add("LicenseLevel", licenseLevel.Value.ToString());}

            // Execute POST and get session token from the response.
            Logger.WriteInfo("Creating session for User ID: {0}.", userId);
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, queryParameters: queryParameters);

            string token = GetToken(response);
            ISession session = new Session(userId, username, licenseLevel.GetValueOrDefault(), isSso.GetValueOrDefault(), token, beginTime, endTime);
            Logger.WriteDebug("Got session token '{0}' for User ID: {1}.", token, userId);

            // Add session to list of created sessions, so we can delete them later.
            Sessions.Add(session);

            return session;
        }

        public ISession AddSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)   // POST /sessions/{userId}
        {
            if (session == null) { throw new ArgumentNullException(nameof(session)); }

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
            var restApi = new RestApiFacade(_address, string.Empty);
            const string sessionTokenHeader = "Session-Token";
            string path = string.Format("{0}/sessions/", SVC_PATH);
            Dictionary<string, string> additionalHeaders = null;

            // We need the Session Token to identify which session to delete.
            if (session?.SessionId != null)
            {
                additionalHeaders = new Dictionary<string, string> {{sessionTokenHeader, session?.SessionId}};
            }

            Logger.WriteInfo("Deleting session '{0}'.", session?.SessionId);
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);

            // Remove the session from the list of created sessions.
            Sessions.Remove(session);
        }

        public ISession GetSession(int? userId)    // GET /sessions/{userId}
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = string.Format("{0}/sessions/{1}", SVC_PATH, (userId.HasValue ? userId.Value.ToString() : string.Empty));

            Logger.WriteTrace("path = '{0}'.", path);
            Logger.WriteInfo("Getting Session for User ID: {0}.", userId);
            ISession session = restApi.SendRequestAndDeserializeObject<Session>(path, RestRequestMethod.GET);
            return session;
        }

        public List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber) // GET /sessions/select?ps={ps}&pn={pn}
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode GetStatus()    // GET /status
        {
            throw new NotImplementedException();
        }

        #endregion Members inherited from IAccessControl
    }
}
