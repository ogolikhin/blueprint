using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Logging;
using RestSharp;
using RestSharp.Authenticators;
using Utilities.Factories;

namespace Utilities.Facades
{
    /// <summary>
    /// The request methods to use for RestApiFacade calls.
    /// </summary>
    public enum RestRequestMethod
    {
        DELETE,
        GET,
        HEAD,
        OPTIONS,
        POST,
        PUT
    }

    /// <summary>
    /// Stores some of the properties that get returned in a REST response and re-packages them in
    /// our own implementation.
    /// </summary>
    public class RestResponse
    {
        private Dictionary<string, object> _headers = new Dictionary<string, object>();

        public string Content { get; set; }
        public string ContentEncoding { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public Exception ErrorException { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Headers { get { return _headers; } }
        public HttpStatusCode StatusCode { get; set; }
    }

    /// <summary>
    /// This class simplifies the job of talking to a REST API.
    /// </summary>
    public class RestApiFacade
    {
        #region Member variables

        private Uri _baseUri;
        private string _username;
        private string _password;
        private string _token;
        private RestResponse _restResponse = new RestResponse();

        #endregion Member variables

        #region Properties

        public string Content               { get { return _restResponse.Content; } }
        public string ContentEncoding       { get { return _restResponse.ContentEncoding; } }
        public long ContentLength           { get { return _restResponse.ContentLength; } }
        public string ContentType           { get { return _restResponse.ContentType; } }
        public Exception ErrorException     { get { return _restResponse.ErrorException; } }
        public string ErrorMessage          { get { return _restResponse.ErrorMessage; } }
        public HttpStatusCode StatusCode    { get { return _restResponse.StatusCode; } }

        #endregion Properties

        #region Private functions

        /// <summary>
        /// Creates a new RestRequest for use by other functions.
        /// </summary>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <returns>An IRestRequest object.</returns>
        private IRestRequest CreateRequest(string resourcePath, RestRequestMethod method, Dictionary<string, string> additionalHeaders = null)
        {
            var client = new RestClient(_baseUri);
            client.Authenticator = new HttpBasicAuthenticator(_username, _password);

            var request = new RestRequest(resourcePath, ConvertToMethod(method));

            if (_token != null)
            {
                request.AddHeader("Authorization", _token);
            }

            if (additionalHeaders != null)
            {
                foreach (var header in additionalHeaders)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }

            return request;
        }

        /// <summary>
        /// Converts the specified RestRequestMethod enum to a RestSharp Method enum.
        /// </summary>
        /// <param name="requestMethod">The source enum to convert.</param>
        /// <returns>A RestSharp Method enum.</returns>
        private static Method ConvertToMethod(RestRequestMethod requestMethod)
        {
            switch (requestMethod)
            {
                case RestRequestMethod.DELETE:
                    return Method.DELETE;
                case RestRequestMethod.GET:
                    return Method.GET;
                case RestRequestMethod.HEAD:
                    return Method.HEAD;
                case RestRequestMethod.OPTIONS:
                    return Method.OPTIONS;
                case RestRequestMethod.POST:
                    return Method.POST;
                case RestRequestMethod.PUT:
                    return Method.PUT;
            }

            throw new ArgumentException(string.Format("Invalid request method '{0}' was passed!", requestMethod.ToString()));
        }

        /// <summary>
        /// Converts an IRestResponse into our own RestResponse object.
        /// </summary>
        /// <param name="restResponse">The response from RestSharp.</param>
        /// <returns>A RestResponse object.</returns>
        private static RestResponse ConvertToRestResponse(IRestResponse restResponse)
        {
            if (restResponse == null) { throw new ArgumentNullException("restResponse"); }

            RestResponse response = new RestResponse()
            {
                Content = restResponse.Content,
                ContentEncoding = restResponse.ContentEncoding,
                ContentLength = restResponse.ContentLength,
                ContentType = restResponse.ContentType,
                ErrorException = restResponse.ErrorException,
                ErrorMessage = restResponse.ErrorMessage,
                StatusCode = restResponse.StatusCode
            };

            foreach (Parameter param in restResponse.Headers)
            {
                response.Headers.Add(param.Name, param.Value);
            }

            return response;
        }

        /// <summary>
        /// Authenticates the user and gets a token.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>The authentication token, or null if no token was received.</returns>
        private string GetUserToken(string username, string password)
        {
            var client = new RestClient(_baseUri);
            client.Authenticator = new HttpBasicAuthenticator(username, password);

            string resource = "authentication/v1/login";
            var authRequest = new RestRequest(resource, Method.GET);
            client.Authenticator.Authenticate(client, authRequest);

            var response = client.Execute(authRequest);

            ThrowIfUnexpectedStatusCode(string.Format("{0}/{1}", _baseUri.ToString().TrimEnd('/'), resource), RestRequestMethod.GET, response.StatusCode);

            // If there is no "Authorization" header, param will be null.
            Parameter param = Enumerable.FirstOrDefault(response.Headers, p =>
            {
                return (p.Name == "Authorization");
            });

            string token = null;

            if (param != null)
            {
                token = (string) param.Value;
                Logger.WriteDebug("Got token '{0}' for user '{1}'.", token, username);
            }
            else
            {
                Logger.WriteWarning("No token was returned for user: '{0}'!", username);
            }

            return token;
        }

        /// <summary>
        /// Throws a WebException derived exception if we got an unexpected status code.
        /// </summary>
        /// <param name="fullAddress">The full address of the REST request.</param>
        /// <param name="method">The method type used.</param>
        /// <param name="statusCode">The status code received.</param>
        /// <param name="expectedStatusCodes">(optional) The expected list of status codes.  By default, only 200 OK is expected.</param>
        /// <exception cref="WebException">An exception derived from WebException.</exception>
        private static void ThrowIfUnexpectedStatusCode(string fullAddress, RestRequestMethod method, HttpStatusCode statusCode, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteDebug("'{0} {1}' got back Status Code: {2}", method.ToString(), fullAddress, statusCode.ToString());

            if (expectedStatusCodes == null) { expectedStatusCodes = new List<HttpStatusCode>() { HttpStatusCode.OK }; }

            if (!expectedStatusCodes.Contains(statusCode))
            {
                throw WebExceptionFactory.Create((int)statusCode);
            }
        }

        #endregion Private functions

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseAddress">The base URI of the REST calls.</param>
        /// <param name="username">Username to authenticate with.</param>
        /// <param name="password">Password to authenticate with.</param>
        public RestApiFacade(string baseAddress, string username, string password)
            : this(new Uri(baseAddress), username, password)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseUri">The base URI of the REST calls.</param>
        /// <param name="username">Username to authenticate with.</param>
        /// <param name="password">Password to authenticate with.</param>
        public RestApiFacade(Uri baseUri, string username, string password)
        {
            _baseUri = baseUri;
            _username = username;
            _password = password;
            _token = GetUserToken(_username, _password);
        }

        /// <summary>
        /// Creates the web request and get the response which is then serialized into the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to be returned by this call.</typeparam>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <returns>The response object(s).</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public T SendRequestAndDeserializeObject<T>(string resourcePath, RestRequestMethod method,
            Dictionary<string, string> additionalHeaders = null,
            List<HttpStatusCode> expectedStatusCodes = null) where T : new()
        {
            var client = new RestClient(_baseUri);
            var request = CreateRequest(resourcePath, method, additionalHeaders);

            try
            {
                var response = client.Execute<T>(request);

                _restResponse = ConvertToRestResponse(response);

                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, expectedStatusCodes);

                return response.Data;   // This will deserialize the data for us.
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e);
            }
        }

        /// <summary>
        /// Creates the web request and get the response object.
        /// </summary>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="fileName">(optional) If you are sending a file, pass the file name here.</param>
        /// <param name="fileContent">(optional) If you are sending a file, pass the file content here.</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <returns>The RestResponse object.</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public RestResponse SendRequestAndGetResponse(string resourcePath, RestRequestMethod method,
            string fileName = null, byte[] fileContent = null,
            Dictionary<string, string> additionalHeaders = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var client = new RestClient(_baseUri);
            var request = CreateRequest(resourcePath, method, additionalHeaders);

            if ((fileName != null) && (fileContent != null))
            {
                request.AddFile(fileName, fileContent, fileName);
            }

            try
            {
                IRestResponse response = client.Execute(request);

                _restResponse = ConvertToRestResponse(response);

                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, expectedStatusCodes);

                return _restResponse;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e);
            }
        }
    }
}
