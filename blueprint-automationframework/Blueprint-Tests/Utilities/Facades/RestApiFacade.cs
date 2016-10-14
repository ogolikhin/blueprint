using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Common;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
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
        PATCH,
        POST,
        PUT
    }

    /// <summary>
    /// Stores some of the properties that get returned in a REST response and re-packages them in
    /// our own implementation.
    /// </summary>
    public class RestResponse
    {
        public string Content { get; set; }
        public string ContentEncoding { get; set; }
        public long ContentLength { get; set; }
        public string ContentType { get; set; }
        public Exception ErrorException { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Headers { get; } = new Dictionary<string, object>();
        public HttpStatusCode StatusCode { get; set; }
        public IEnumerable<byte> RawBytes { get; set; }
    }

    /// <summary>
    /// Stores some of the properties that get send in a REST request and re-packages them in
    /// our own implementation.
    /// TODO: May need to remove it if there is a bug on PostNovaFile_MultiPartMime_FileExists test cases
    /// </summary>
    public class RestRequest
    {
        public long ContentLength { get; set; }
    }

    /// <summary>
    /// This class simplifies the job of talking to a REST API.
    /// </summary>
    public class RestApiFacade
    {
        #region Member variables

        private readonly Uri _baseUri;
        private readonly string _username;
        private readonly string _password;
        private readonly string _token;
        private RestResponse _restResponse = new RestResponse();
        private RestRequest _restRequest = new RestRequest();

        #endregion Member variables

        #region Properties

        public string Content => _restResponse.Content;
        public string ContentEncoding => _restResponse.ContentEncoding;
        public long ContentLength => _restResponse.ContentLength;
        public string ContentType => _restResponse.ContentType;
        public Exception ErrorException => _restResponse.ErrorException;
        public string ErrorMessage => _restResponse.ErrorMessage;
        public IEnumerable<byte> RawBytes => _restResponse.RawBytes;
        public HttpStatusCode StatusCode => _restResponse.StatusCode;

        public long ReqContentLength => _restRequest.ContentLength;
        
        #endregion Properties

        #region Private functions

        /// <summary>
        /// Creates a new RestRequest for use by other functions.
        /// </summary>
        /// <param name="client">The RestClient that you will be using with this request.</param>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) List of query parameters to add to the request.</param>
        /// <param name="cookies">(optional) List of cookies to add to the request.</param>
        /// <param name="requestTimeout">(optional) The request timeout in milliseconds.  Default timeout is 5 mins.</param>
        /// <returns>An IRestRequest object.</returns>
        private IRestRequest CreateRequest(RestClient client,
            string resourcePath,
            RestRequestMethod method,
            Dictionary<string, string> additionalHeaders = null,
            Dictionary<string, string> queryParameters = null,
            Dictionary<string, string> cookies = null,
            int requestTimeout = 300000)
        {
            // Only use BasicAuthenticator if we passed a Username & Password but no Token.
            // NOTE: This should only be needed in the OpenApi Login REST call.
            if (string.IsNullOrWhiteSpace(_token) && !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password))
            {
                client.Authenticator = new HttpBasicAuthenticator(_username, _password);
            }

            var request = new RestSharp.RestRequest(resourcePath, ConvertToMethod(method));

            if (_token != null)
            {
                Logger.WriteTrace("**** Adding Authorization headers.");

                if (_token.StartsWithOrdinal("BlueprintToken"))
                {
                    request.AddHeader("Authorization", _token); // This is for old OpenAPI.
                }
                else
                {
                    request.AddHeader("Session-Token", _token); // This is for new AccessControl.
                }
            }

            if (additionalHeaders == null)
            {
                additionalHeaders = new Dictionary<string, string>();
            }

            additionalHeaders.Add("Accept", "application/json");

            foreach (var header in additionalHeaders)
            {
                // First if the header already exists, remove it so we can replace it.
                request.Parameters.RemoveAll(p => { return (p.Name == header.Key); });

                Logger.WriteTrace("**** Adding additional header '{0}'.", header.Key);
                request.AddHeader(header.Key, header.Value);
            }

            if (queryParameters != null)
            {
                foreach (var queryParameter in queryParameters)
                {
                    Logger.WriteTrace("**** Adding queryparameter '{0}'.", queryParameter.Key);
                    request.AddQueryParameter(queryParameter.Key, queryParameter.Value);
                }
            }

            if (cookies != null)
            {
                foreach (var cookie in cookies)
                {
                    Logger.WriteTrace("**** Adding cookie '{0}'.", cookie.Key);
                    request.AddParameter(cookie.Key, cookie.Value, ParameterType.Cookie);
                }
            }

            request.Timeout = requestTimeout;
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
                case RestRequestMethod.PATCH:
                    return Method.PATCH;
                case RestRequestMethod.POST:
                    return Method.POST;
                case RestRequestMethod.PUT:
                    return Method.PUT;
            }

            throw new ArgumentException(I18NHelper.FormatInvariant("Invalid request method '{0}' was passed!", requestMethod.ToString()));
        }

        /// <summary>
        /// Converts an IRestResponse into our own RestResponse object.
        /// </summary>
        /// <param name="restResponse">The response from RestSharp.</param>
        /// <returns>A RestResponse object.</returns>
        private static RestResponse ConvertToRestResponse(IRestResponse restResponse)
        {
            ThrowIf.ArgumentNull(restResponse, nameof(restResponse));

            var response = new RestResponse
            {
                Content = restResponse.Content,
                ContentEncoding = restResponse.ContentEncoding,
                ContentLength = restResponse.ContentLength,
                ContentType = restResponse.ContentType,
                ErrorException = restResponse.ErrorException,
                ErrorMessage = restResponse.ErrorMessage,
                StatusCode = restResponse.StatusCode,
                RawBytes = restResponse.RawBytes
            };

            foreach (var param in restResponse.Headers)
            {
                response.Headers.Add(param.Name, param.Value);
            }

            return response;
        }

        /// <summary>
        /// Converts an IRestRequest into our own RestRequest object.
        /// </summary>
        /// <param name="fileName">The filename in the request.</param>
        /// <param name="restRequest">The request from RestSharp.</param>
        /// <returns>A RestRequest object.</returns>
        private static RestRequest ConvertToRestRequest(string fileName, IRestRequest restRequest)
        {
            ThrowIf.ArgumentNull(restRequest, nameof(restRequest));

            var request = new RestRequest();
            if (restRequest.Parameters.Exists(p => p.Type.Equals(ParameterType.RequestBody)) && restRequest.Parameters.Exists(p=>p.Value.Equals(fileName)))
            {
                request.ContentLength = ((byte[]) restRequest.Parameters.First(p => p.Type.Equals(ParameterType.RequestBody)).Value).ToArray().Length;
            }
            if (restRequest.Files.Any())
            {
                request.ContentLength = restRequest.Files[0].ContentLength + 180 + restRequest.Files[0].FileName.Length*2;
            }            
            return request; 
        }

        /// <summary>
        /// Throws a WebException derived exception if we got an unexpected status code.
        /// </summary>
        /// <param name="fullAddress">The full address of the REST request.</param>
        /// <param name="method">The method type used.</param>
        /// <param name="statusCode">The status code received.</param>
        /// <param name="exceptionMsg">The exception message.</param>
        /// <param name="restResponse">(optional) The REST response to include in the exception (if thrown).</param>
        /// <param name="expectedStatusCodes">(optional) The expected list of status codes.  By default, only 200 OK is expected.</param>
        /// <exception cref="WebException">An exception derived from WebException.</exception>
        private static void ThrowIfUnexpectedStatusCode(
            string fullAddress,
            RestRequestMethod method,
            HttpStatusCode statusCode,
            string exceptionMsg,
            RestResponse restResponse = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteDebug("'{0} {1}' got back Status Code: {2}", method.ToString(), fullAddress, statusCode.ToString());

            if (expectedStatusCodes == null) { expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK }; }

            if (!expectedStatusCodes.Contains(statusCode))
            {
                throw WebExceptionFactory.Create((int)statusCode, exceptionMsg, restResponse);
            }
        }

        #endregion Private functions

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseAddress">The base URI of the REST calls.</param>
        /// <param name="token">(optional) The user token to use for the request.  By default, if null was passed, we get a valid token for the user. 
        /// If you don't want to use a token, you should pass an empty string here.</param>
        public RestApiFacade(string baseAddress, string token = null) : this(new Uri(baseAddress), token)
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseAddress">The base URI of the REST calls.</param>
        /// <param name="username">Username to authenticate with.</param>
        /// <param name="password">Password to authenticate with.</param>
        /// <param name="token">The user token to use for the request.  If you don't want to use a token, you should pass an empty string here.</param>
        public RestApiFacade(string baseAddress, string username, string password, string token)
            : this(new Uri(baseAddress), token)
        {
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseUri">The base URI of the REST calls.</param>
        /// <param name="token">(optional) The user token to use for the request.  If you don't want to use a token, you should pass an empty string here.</param>
        public RestApiFacade(Uri baseUri, string token)
        {
            _baseUri = baseUri;
            _token = token;
        }

        /// <summary>
        /// Creates the web request and get the response which is then serialized into the specified type.
        /// </summary>
        /// <typeparam name="T1">The type of object to be returned by this call.</typeparam>
        /// <typeparam name="T2">The type of object to send for the web request.</typeparam>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="jsonObject">The concrete (non-interface) object to serialize and send in the request body.</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) Add query parameters</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <param name="cookies">(optional) Add cookies</param>
        /// <param name="shouldControlJsonChanges">(optional) If true method will check that JSON returned by server corresponds to the current model. If JSON has been changed method will throw a FormatException. False by default.</param>
        /// <returns>The response object(s).</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        /// <exception cref="FormatException">A FormatException if JSON has been changed.</exception>
        public T1 SendRequestAndDeserializeObject<T1,T2>(
            string resourcePath, 
            RestRequestMethod method,
            T2 jsonObject,
            Dictionary<string, string> additionalHeaders = null,
            Dictionary<string, string> queryParameters = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            Dictionary<string, string> cookies = null,
            bool shouldControlJsonChanges = false)
            where T1 : new()
            where T2 : new()
        {
            var client = new RestClient(_baseUri);

            var request = CreateRequest(client, resourcePath, method, additionalHeaders, queryParameters, cookies);

            if (jsonObject != null)
            {
                request.JsonSerializer = new Deserialization.CustomJsonSerializer();
                request.AddJsonBody(jsonObject);
            }

            IRestResponse response = null;

            try
            {
                response = client.Execute(request);

                _restResponse = ConvertToRestResponse(response);
                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, _restResponse.ErrorMessage, _restResponse, expectedStatusCodes);

                // Derialization
                var result = JsonConvert.DeserializeObject<T1>(response.Content);

                ////try to serialize and compare
                if (shouldControlJsonChanges)
                {
                    JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    };//TODO : move this to each specific class!
                    
                    string serializeObject = JsonConvert.SerializeObject(result, jsonSerializerSettings);
                    bool isJSONChanged = !(string.Equals(response.Content, serializeObject, StringComparison.OrdinalIgnoreCase));

                    if (isJSONChanged)
                    {
                        string msg = I18NHelper.FormatInvariant("JSON for {0} has been changed!\r\nReceived JSON: {1}\r\nSerialized JSON: {2}",
                            typeof(T1).ToString(), response.Content, serializeObject);
                        throw new FormatException(msg);
                    }
                }
                ////

                Logger.WriteDebug("SendRequestAndDeserializeObject() got Status Code '{0}' for user '{1}'.", response.StatusCode, _username);

                Logger.WriteDebug("Deserialized Response Content: {0}", response.Content);

                return result;   // This will deserialize the data for us.
            }
            catch (JsonReaderException)
            {
                Logger.WriteError("Error while deserializing the response!");
                Logger.WriteError("The server returned:  {0}", response?.Content);
                throw;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e, _restResponse);
            }
        }

        /// <summary>
        /// Creates the web request and get the response which is then serialized into the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to be returned by this call.</typeparam>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) Add query parameters</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <param name="cookies">(optional) Add cookies</param>
        /// <param name="shouldControlJsonChanges">(optional) If true method will check that JSON returned by server corresponds to the current model. If JSON has been changed method will throw a FormatException. False by default.</param>
        /// <returns>The response object(s).</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        /// <exception cref="FormatException">A FormatException if JSON has been changed.</exception>
        public T SendRequestAndDeserializeObject<T>(
           string resourcePath,
           RestRequestMethod method,
           Dictionary<string, string> additionalHeaders = null,
           Dictionary<string, string> queryParameters = null,
           List<HttpStatusCode> expectedStatusCodes = null,
           Dictionary<string, string> cookies = null,
           bool shouldControlJsonChanges = false) where T : new()
        {
            return SendRequestAndDeserializeObject<T, List<string>>(resourcePath,
                method,
                null,
                additionalHeaders,
                queryParameters,
                expectedStatusCodes,
                cookies,
                shouldControlJsonChanges);
        }

        /// <summary>
        /// Creates the web request and get the response object.
        /// </summary>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="fileName">(optional) If you are sending a file, pass the file name here.</param>
        /// <param name="fileContent">(optional) If you are sending a file, pass the file content here.</param>
        /// <param name="contentType">(optional) The Mime content type.</param>
        /// <param name="useMultiPartMime">(optional) Use multi-part mime for the request</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) Add query parameters</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <param name="cookies">(optional) Add cookies</param>
        /// <returns>The RestResponse object.</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public RestResponse SendRequestAndGetResponse(
            string resourcePath,
            RestRequestMethod method,
            string fileName = null,
            byte[] fileContent = null,
            string contentType = null,
            bool useMultiPartMime = false,
            Dictionary<string, string> additionalHeaders = null,
            Dictionary<string, string> queryParameters = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            Dictionary<string, string> cookies = null)
        {
            Logger.WriteTrace("Base URI for REST request is: {0}", _baseUri);
            var client = new RestClient(_baseUri);
            var request = CreateRequest(client, resourcePath, method, additionalHeaders, queryParameters, cookies);

            if (fileName != null && (fileContent != null))
            {
                Logger.WriteTrace("**** Adding file '{0}'.", fileName);

                if (useMultiPartMime)
                {
                    request.AddFile(fileName, fileContent, fileName, contentType);
                }
                else
                {
                    request.AddParameter(contentType, fileContent, ParameterType.RequestBody);
                }
            }

            try
            {
                var response = client.Execute(request);
                Logger.WriteDebug("SendRequestAndGetResponse() got Status Code '{0}' for user '{1}'.",
                    response.StatusCode, _username);

                _restResponse = ConvertToRestResponse(response);
                _restRequest = ConvertToRestRequest(fileName, request);                    
                              
                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, _restResponse.ErrorMessage, _restResponse, expectedStatusCodes);

                return _restResponse;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e, _restResponse);
            }
        }

        /// <summary>
        /// Creates the web request with a specific body and get the response object.
        /// </summary>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="requestBody">The body to send in the request.</param>
        /// <param name="contentType">(optional) The Mime content type.</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) Add query parameters</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <param name="cookies">(optional) Add cookies</param>
        /// <returns>The RestResponse object.</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public RestResponse SendRequestBodyAndGetResponse(
            string resourcePath,
            RestRequestMethod method,
            string requestBody,
            string contentType = "text/plain",
            Dictionary<string, string> additionalHeaders = null,
            Dictionary<string, string> queryParameters = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            Dictionary<string, string> cookies = null)
        {
            ThrowIf.ArgumentNull(requestBody, nameof(requestBody));

            Logger.WriteTrace("Base URI for REST request is: {0}", _baseUri);
            var client = new RestClient(_baseUri);
            var request = CreateRequest(client, resourcePath, method, additionalHeaders, queryParameters, cookies);
            request.AddParameter(contentType, requestBody, ParameterType.RequestBody);

            try
            {
                var response = client.Execute(request);
                Logger.WriteDebug("SendRequestAndGetResponse() got Status Code '{0}' for user '{1}'.",
                    response.StatusCode, _username);

                _restResponse = ConvertToRestResponse(response);
                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, _restResponse.ErrorMessage, _restResponse, expectedStatusCodes);

                return _restResponse;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e, _restResponse);
            }
        }

        /// <summary>
        /// Creates the web request with a JSON body and get the response object.
        /// </summary>
        /// <param name="resourcePath">The path for the REST request (i.e. not including the base URI).</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="queryParameters">(optional) Add query parameters</param>
        /// <param name="bodyObject">(optional) An object to send in the HTTP body of the request.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <param name="cookies">(optional) Add cookies</param>
        /// <returns>The RestResponse object.</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public RestResponse SendRequestAndGetResponse<T>(
            string resourcePath, 
            RestRequestMethod method,
            Dictionary<string, string> additionalHeaders = null,
            Dictionary<string, string> queryParameters = null,
            T bodyObject = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            Dictionary<string, string> cookies = null) where T : class
        {
            var client = new RestClient(_baseUri);
            var request = CreateRequest(client, resourcePath, method, additionalHeaders, queryParameters, cookies);

            if (bodyObject != null)
            {
                request.JsonSerializer = new Deserialization.CustomJsonSerializer();
                request.AddJsonBody(bodyObject);
            }

            try
            {
                var response = client.Execute(request);
                Logger.WriteDebug("SendRequestAndGetResponse() got Status Code '{0}' for user '{1}'.",
                    response.StatusCode, _username);

                _restResponse = ConvertToRestResponse(response);
                ThrowIfUnexpectedStatusCode(resourcePath, method, _restResponse.StatusCode, _restResponse.ErrorMessage, _restResponse, expectedStatusCodes);

                return _restResponse;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e, _restResponse);
            }
        }
    }
}
