using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class AdminStore : IAdminStore
    {
        private const string SVC_PATH = "svc/adminstore";
        private const string TOKEN_HEADER = BlueprintToken.ACCESS_CONTROL_TOKEN_HEADER;

        private string _address = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the AdminStore service.</param>
        public AdminStore(string address)
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

        #region Members inherited from IAdminStore

        public List<ISession> Sessions { get; } = new List<ISession>();

        public ISession AddSession(ISession session, bool? force = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            throw new NotImplementedException();
        }

        public ISession AddSession(string username = null, string password = null, bool? force = default(bool?),
            List<HttpStatusCode> expectedStatusCodes = null, IServiceErrorMessage expectedServiceErrorMessage = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/sessions", SVC_PATH);

            string encodedUsername = HashingUtilities.EncodeTo64UTF8(username);
            string encodedPassword = HashingUtilities.EncodeTo64UTF8(password);
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "login", encodedUsername } };

            if (force != null)
            {
                queryParameters.Add("force", force.ToString());
            }
            
            try
            {
                Logger.WriteInfo("Adding session for user '{0}'...", username);
                RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, additionalHeaders, queryParameters,
                    encodedPassword, expectedStatusCodes);

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
                    Assert.That(expectedServiceErrorMessage.Equals(serviceErrorMessage),
                        "Response message is different from expected!");
                }

                throw;
            }
        }

        public void DeleteSession(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/sessions", SVC_PATH);

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Deleting session '{0}'...", session?.SessionId);
            restApi.SendRequestAndGetResponse(path, RestRequestMethod.DELETE, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);
        }

        public ISession GetSession(int? userId)
        {
            throw new NotImplementedException();
        }

        public List<ISession> GetSession(string adminToken, uint pageSize, uint pageNumber)
        {
            throw new NotImplementedException();
        }

        public HttpStatusCode GetStatus()
        {
            throw new NotImplementedException();
        }

        #endregion Members inherited from IAdminStore
    }
}
