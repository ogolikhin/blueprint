﻿using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;
using Model.Factories;

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

        #region Members inherited from IAdminStore

        /// <seealso cref="IAdminStore.Sessions"/>
        public List<ISession> Sessions { get; } = new List<ISession>();

        /// <seealso cref="IAdminStore.AddSsoSession(string, string, bool?, List{HttpStatusCode})"/>
        public ISession AddSsoSession(string username, string samlResponse, bool? force = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/sessions/sso", SVC_PATH);

            string encodedSamlResponse = HashingUtilities.EncodeTo64UTF8(samlResponse);
            Dictionary<string, string> additionalHeaders = new Dictionary<string, string> { { "Content-Type", "Application/json" } };
            Dictionary<string, string> queryParameters = null;

            if (force != null)
            {
                queryParameters = new Dictionary<string, string> { { "force", force.ToString() } };
            }

            Logger.WriteInfo("Adding SSO session for user '{0}'...", username);
            RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.POST, additionalHeaders, queryParameters,
                encodedSamlResponse, expectedStatusCodes);

            string token = GetToken(response);

            ISession session = new Session { UserName = username, IsSso = true, SessionId = token };
            Logger.WriteDebug("Got session token '{0}' for User: {1}.", token, username);

            // Add session to list of created sessions, so we can delete them later.
            Sessions.Add(session);
            Logger.WriteDebug("Content = '{0}'", restApi.Content);

            return session;
        }

        /// <seealso cref="IAdminStore.AddSession(string, string, bool?, List{HttpStatusCode}, IServiceErrorMessage)"/>
        public ISession AddSession(string username = null, string password = null, bool? force = null,
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

        /// <seealso cref="IAdminStore.DeleteSession(ISession, List{HttpStatusCode})"/>
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

        /// <seealso cref="IAdminStore.GetLoginUser(string, List{HttpStatusCode})"/>
        public IUser GetLoginUser(string token, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, token: token);
            string path = I18NHelper.FormatInvariant("{0}/users/loginuser", SVC_PATH);

            try
            {
                Logger.WriteInfo("Getting logged in user's info...");
                RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

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
                user.SetToken(token: token);
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

        /// <seealso cref="IAdminStore.GetStatus"/>
        public string GetStatus(string preAuthorizedKey = CommonConstants.PreAuthorizedKeyForStatus, List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/status", SVC_PATH);

            var queryParameters = new Dictionary<string, string>();

            if (preAuthorizedKey != null)
            {
                queryParameters.Add("preAuthorizedKey", preAuthorizedKey);
            }

            Logger.WriteInfo("Getting AdminStore status...");
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
            return response.Content;
        }

        /// <seealso cref="IAdminStore.GetStatusUpcheck"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/status/upcheck", SVC_PATH);

            Logger.WriteInfo("Getting AdminStore status upcheck...");
            var response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);
            return response.StatusCode;
        }

        /// <seealso cref="IAdminStore.GetSettings(ISession, List{HttpStatusCode})"/>
        public Dictionary<string, object> GetSettings(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/config/settings", SVC_PATH);

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting settings...");
            RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
        }

        /// <seealso cref="IAdminStore.GetConfigJs(ISession, List{HttpStatusCode})"/>
        public string GetConfigJs(ISession session, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/config/config.js", SVC_PATH);

            Dictionary<string, string> additionalHeaders = null;

            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }

            Logger.WriteInfo("Getting config.js...");
            RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, additionalHeaders: additionalHeaders, expectedStatusCodes: expectedStatusCodes);
            return response.Content;
        }

        /// <seealso cref="IAdminStore.GetLicenseTransactions(int, ISession, List{HttpStatusCode})"/>
        public IList<LicenseActivity> GetLicenseTransactions(int numberOfDays, ISession session = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            RestApiFacade restApi = new RestApiFacade(_address, string.Empty);
            string path = I18NHelper.FormatInvariant("{0}/licenses/transactions", SVC_PATH);

            Dictionary<string, string> queryParameters = new Dictionary<string, string> { { "days", numberOfDays.ToString(System.Globalization.CultureInfo.InvariantCulture)} };
            Dictionary<string, string> additionalHeaders = null;
            if (session != null)
            {
                additionalHeaders = new Dictionary<string, string> { { TOKEN_HEADER, session.SessionId } };
            }
            try
            {
                Logger.WriteInfo("Getting list of License Transactions...");
                RestResponse response = restApi.SendRequestAndGetResponse(path, RestRequestMethod.GET, additionalHeaders: additionalHeaders,
                queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes);
                return JsonConvert.DeserializeObject<List<LicenseActivity>>(response.Content);
            }
            catch (WebException ex)
            {
                Logger.WriteError("Content = '{0}'", restApi.Content);
                Logger.WriteError("Error while getting list of License Transactions - {0}", ex.Message);
                throw;
            }
        }

        /// <seealso cref="IAdminStore.ResetPassword(IUser, string, List{HttpStatusCode})"/>
        public void ResetPassword(IUser user, string newPassword, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var path = I18NHelper.FormatInvariant("{0}/users/reset", SVC_PATH);

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

            var restApi = new RestApiFacade(_address, string.Empty);
            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                queryParameters: queryParameters,
                bodyObject: bodyObject,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Members inherited from IAdminStore
    }
}
