using System;
using System.Collections.Generic;
using System.Net;
using Common;
using Utilities;
using Utilities.Facades;
using Model.Factories;

namespace Model.Impl
{
    public class BlueprintServer : NovaServiceBase, IBlueprintServer
    {
        #region Public functions

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The Blueprint server address (ex. 'http://blueprint:8080').</param>
        public BlueprintServer(string address)
        {
            Address = address;
        }

        /// <summary>
        /// Gets the keys/values to add to the HTTP headers with the user's token added.
        /// </summary>
        /// <param name="user">The user that has the token.</param>
        /// <returns>The keys/values to add to the HTTP headers with the user's token added.</returns>
        public static Dictionary<string, string> GetTokenHeader(IUser user)
        {
            if ((user == null) || (user.Token == null)) { throw new ArgumentException("The user argument is null or has no Token!"); }

            var dict = new Dictionary<string, string> {{user.Token.OpenApiTokenHeader, user.Token.OpenApiToken}};
            return dict;
        }

        /// <summary>
        /// Returns the token from the IUser object.  If the user doesn't have a token yet, a token is first added to the IUser object.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">The user that has the token or who get a new token.</param>
        /// <param name="forceNew">Pass true to force a new token to be added to the user.</param>
        /// <returns>The user's token.</returns>
        public static IBlueprintToken GetUserToken(string address, IUser user, bool forceNew = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            if (forceNew || (user.Token == null))
            {
                var resp = LoginUsingBasicAuthorization(address, user);
                user.Token = GetTokenFromResponse(resp);
                Logger.WriteTrace("Disposing HttpWebResponse from LoginUsingBasicAuthorization().");
            }

            return user.Token;
        }

        #endregion Public functions

        #region Inherited from IBlueprintServer

        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="user">The user whose credentials will be used to login.  This users token will be updated with the new token received from Blueprint.</param>
        /// <param name="token">(optional) The user token to use for the request.  By default, if null was passed, we get a valid token for the user.
        /// If you don't want to use a token, you should pass an empty string here.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The RestResponse received from the server.</returns>
        public RestResponse LoginUsingBasicAuthorization(IUser user, string token = null, uint maxRetries = 1)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var response = LoginUsingBasicAuthorization(Address, user, token, maxRetries);
            var newToken = GetTokenFromResponse(response);

            if (newToken != null)
            {
                user.SetToken(newToken.OpenApiToken);
            }

            return response;
        }

        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">The user whose credentials will be used to login.</param>
        /// <param name="token">(optional) The user token to use for the request.  By default, if null was passed, we get a valid token for the user.
        /// If you don't want to use a token, you should pass an empty string here.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The RestResponse received from the server.</returns>
        public static RestResponse LoginUsingBasicAuthorization(string address, IUser user, string token = null, uint maxRetries = 1)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            RestResponse response = null;
            var restApi = new RestApiFacade(address, user.Username, user.Password, token);

            for (uint attempt = 1; attempt <= maxRetries + 1; ++attempt)
            {
                try
                {
                    response = restApi.SendRequestAndGetResponse(RestPaths.OpenApi.LOGIN, RestRequestMethod.GET);
                    break;
                }
                catch (WebException e)
                {
                    if (e is OperationTimedOutException || e is ConnectionClosedException ||
                        e is RequestAbortedException)
                    {
                        Logger.WriteError("Login for user: {0} timed out on attempt {1}!  {2}", user.Username, attempt,
                            e.Message);

                        if (attempt > maxRetries)
                        {
                            Logger.WriteWarning("Max retries exceeded.  Throwing exception.");
                            throw;
                        }
                    }
                    else
                    {
                        Logger.WriteError("Login failed for user {0}!  {1}", user.Username, e.Message);
                        throw;
                    }
                }
            }

            if (response == null) { Logger.WriteWarning("LoginUsingBasicAuthorization is returning null!!"); }
            return response;
        }

        /// <seealso cref="IBlueprintServer.GetStatus(string, List{HttpStatusCode})"/>
        public string GetStatus(string preAuthorizedKey = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatus(RestPaths.Svc.STATUS, preAuthorizedKey, expectedStatusCodes);
        }

        /// <seealso cref="IBlueprintServer.GetStatusUpcheck(List{HttpStatusCode})"/>
        public HttpStatusCode GetStatusUpcheck(List<HttpStatusCode> expectedStatusCodes = null)
        {
            return GetStatusUpcheck(RestPaths.Svc.Status.UPCHECK, expectedStatusCodes);
        }

        /// <seealso cref="IBlueprintServer.GetIsFedAuthenticationEnabledDB()"/>
        public bool GetIsFedAuthenticationEnabledDB()
        {
            bool result = false;

            using (var database = DatabaseFactory.CreateDatabase())
            {
                string query = I18NHelper.FormatInvariant(
                    "SELECT Enabled FROM [dbo].[FederatedAuthentications] WHERE InstanceId = 1");
                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                {
                    database.Open();

                    try
                    {
                        result = (bool)cmd.ExecuteScalar();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("SQL query didn't get processed. Exception details = {0}", ex);
                    }
                }
            }
            return result;
        }

        /// <seealso cref="IBlueprintServer.SetIsFedAuthenticationEnabledDB(bool)"/>
        public void SetIsFedAuthenticationEnabledDB(bool isFedAuthenticationEnabledDB)
        {
            using (var database = DatabaseFactory.CreateDatabase())
            {
                string query = I18NHelper.FormatInvariant(
                    "UPDATE [dbo].[FederatedAuthentications] SET Enabled = {0} WHERE InstanceId = 1",
                    Convert.ToInt32(isFedAuthenticationEnabledDB));
                Logger.WriteDebug("Running: {0}", query);

                using (var cmd = database.CreateSqlCommand(query))
                {
                    database.Open();

                    try
                    {
                        cmd.ExecuteScalar();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Logger.WriteError("SQL query didn't get processed. Exception details = {0}", ex);
                    }
                }
            }
        }

        #endregion Inherited from IBlueprintServer

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting anything created by this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(BlueprintServer), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: Delete any resources created by this class.
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting anything created by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable

        #region Private functions

        /// <summary>
        /// Gets the token from the specified HttpWebResponse.
        /// </summary>
        /// <param name="response">The response from a login request.</param>
        /// <returns>A token.</returns>
        private static IBlueprintToken GetTokenFromResponse(RestResponse response)
        {
            string tokenString = null;

            if (response.Headers.ContainsKey(BlueprintToken.OPENAPI_TOKEN_HEADER))
            {
                tokenString = (string) response.Headers[BlueprintToken.OPENAPI_TOKEN_HEADER];
            }

            var token = new BlueprintToken(openApiToken: tokenString);
            return token;
        }

        #endregion Private functions
    }
}
