using System;

namespace Model.Impl
{
    public class BlueprintToken : IBlueprintToken
    {
        public const string ACCESS_CONTROL_TOKEN_HEADER = "Session-Token";
        public const string OPENAPI_TOKEN_HEADER = "Authorization";

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
                if ((value != null) && !value.StartsWith("BlueprintToken") && (value.Length == 32))
                {
                    _accessControlToken = value;
                }
                else if (value == string.Empty)
                {
                    // This is a hack for cases when you don't have a valid token, but you don't want Model classes to automatically authenticate the user.
                    _accessControlToken = value;
                }
                else
                {
                    throw new ArgumentException("The specified token is not a valid Access Control token!");
                }
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
                if ((value != null) && value.StartsWith("BlueprintToken"))
                {
                    _openApiToken = value;
                }
                else if (value == string.Empty)
                {
                    // This is a hack for cases when you don't have a valid token, but you don't want Model classes to automatically authenticate the user.
                    _accessControlToken = value;
                }
                else
                {
                    throw new ArgumentException("The specified token is not a valid OpenAPI token!");
                }
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
