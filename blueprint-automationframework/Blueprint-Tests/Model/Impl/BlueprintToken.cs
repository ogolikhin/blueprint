using System;
using Common;
using Utilities;

namespace Model.Impl
{
    public class BlueprintToken : IBlueprintToken
    {
        public const string ACCESS_CONTROL_TOKEN_HEADER = "Session-Token";
        public const string OPENAPI_TOKEN_HEADER = "Authorization";

        /// <summary>
        /// This is the string that all OpenApi tokens start with.
        /// </summary>
        public const string OPENAPI_START_OF_TOKEN = "BlueprintToken";

        /// <summary>
        /// Setting the token to this will prevent us from sending any Authentication headers in REST requests.
        /// </summary>
        public const string NO_TOKEN = null;

        /// <summary>
        /// Setting the token to this will send an Authentication header with no token, which should usually produce a 400 Bad Request.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]   // Ignore this warning.
        public readonly string EMPTY_TOKEN = string.Empty;

        private string _accessControlToken;
        private string _openApiToken;

        /// <summary>
        /// Returns the name of the HTTP Header for Access Control tokens.
        /// </summary>
        public string AccessControlTokenHeader { get { return ACCESS_CONTROL_TOKEN_HEADER; } }

        /// <summary>
        /// Returns the name of the HTTP Header for OpenAPI tokens.
        /// </summary>
        public string OpenApiTokenHeader { get { return OPENAPI_TOKEN_HEADER; } }

        /// <summary>
        /// Gets/sets the AccessControl token string.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]   // Ignore this warning.
        public string AccessControlToken
        {
            get { return _accessControlToken; }
            set
            {
                Logger.WriteDebug("Setting AccessControlToken to: '{0}'", value);
                _accessControlToken = value;
            }
        }

        /// <summary>
        /// Gets/sets the OpenAPI token string.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]   // Ignore this warning.
        public string OpenApiToken
        {
            get { return _openApiToken; }
            set
            {
                Logger.WriteDebug("Setting OpenApiToken to: {0}", value);
                _openApiToken = value;
            }
        }

        /// <summary>
        /// Sets the token (either for AccessControl or OpenAPI, depending on the token format).
        /// </summary>
        /// <param name="token">A token string from AccessControl/AdminStore or OpenAPI.</param>
        /// <exception cref="ArgumentException">If an invalid token string was passed.</exception>
        public void SetToken(string token)
        {
            ThrowIf.ArgumentNull(token, nameof(token));

            if (token.StartsWithOrdinal(OPENAPI_START_OF_TOKEN))
            {
                OpenApiToken = token;
            }
            else
            {
                AccessControlToken = token;
            }
        }

        /// <summary>
        /// Constructor that sets the token to the specified token string.
        /// </summary>
        /// <param name="accessControlToken">The AccessControl token string.</param>
        /// <param name="openApiToken">(optional) The OpenAPI token string.</param>
        /// <exception cref="ArgumentException">If the specified tokens are invalid.</exception>
        public BlueprintToken(string accessControlToken = null, string openApiToken = null)
        {
            if (accessControlToken != null)
            {
                AccessControlToken = accessControlToken;
            }

            if (openApiToken != null)
            {
                OpenApiToken = openApiToken;
            }
        }
    }
}
