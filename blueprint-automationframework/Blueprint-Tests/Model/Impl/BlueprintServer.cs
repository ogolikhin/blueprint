using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Logging;
using Model.Facades;
using Newtonsoft.Json;
using Utilities;


namespace Model.Impl
{
    public class BlueprintServer : IBlueprintServer
    {
        #region Properties and member variables.

        private string _address;

        /// <summary>
        /// Gets/sets the URL address of the server.  Note: any trailing '/' characters will be removed.
        /// </summary>
        public string Address
        {
            get { return _address; }
            set { _address = (value != null) ? value.TrimEnd('/') : null; }
        }

        #endregion Properties and member variables.


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

            var dict = new Dictionary<string, string>() {{"Authorization", user.Token.TokenString}};
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
            if (user == null) { throw new ArgumentNullException("user"); }

            if (forceNew || (user.Token == null))
            {
                using (HttpWebResponse resp = LoginUsingBasicAuthorization(address, user))
                {
                    user.Token = getTokenFromResponse(resp);
                    Logger.WriteTrace("Disposing HttpWebResponse from LoginUsingBasicAuthorization().");
                }
            }

            return user.Token;
        }

        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="user">The user whose credentials will be used to login.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The HttpWebResponse received from the server.</returns>
        public HttpWebResponse LoginUsingBasicAuthorization(IUser user, uint maxRetries = 1)
        {
            return LoginUsingBasicAuthorization(Address, user, maxRetries);
        }

        /// <summary>
        /// Login to the Blueprint server using the Basic authorization method.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">The user whose credentials will be used to login.</param>
        /// <param name="maxRetries">(optional) The maximum number of times to retry the login in case we get socket timeouts.</param>
        /// <returns>The HttpWebResponse received from the server.</returns>
        public static HttpWebResponse LoginUsingBasicAuthorization(string address, IUser user, uint maxRetries = 1)
        {
            if (user == null) { throw new ArgumentNullException("user"); }

            HttpWebResponse response = null;

            for (uint attempt = 1; attempt <= maxRetries + 1; ++attempt)
            {
                try
                {
                    response = WebRequestFacade.CreateWebRequestAndGetResponse(
                        address + "/authentication/v1/login", "GET",
                        getBasicAuthorizationHeader(user)
                        );
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

        #endregion Public functions


        #region Private functions

        /// <summary>
        /// Gets the HTTP headers necessary for basic authorization using the credentials of the user that is passed in.
        /// </summary>
        /// <param name="user">The user whose credentials will be used in the HTTP header.</param>
        /// <returns>A Dictionary of keys/values to add to the HTTP request header.</returns>
        private static Dictionary<string, string> getBasicAuthorizationHeader(IUser user)
        {
            Logger.WriteTrace("  Adding username='{0}' & password='{1}' to header.", user.Username, user.Password);
            var dict = new Dictionary<string, string>() { { "Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", user.Username, user.Password))) } };
            return dict;
        }

        /// <summary>
        /// Gets the token from the specified HttpWebResponse.
        /// </summary>
        /// <param name="response">The response from a login request.</param>
        /// <returns>A token.</returns>
        private static IBlueprintToken getTokenFromResponse(HttpWebResponse response)
        {
            IBlueprintToken token = new BlueprintToken(response.Headers["Authorization"]);
            return token;
        }

        #endregion Private functions
    }
}
