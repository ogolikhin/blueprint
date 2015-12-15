using System;
using System.Net;
using Logging;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Model.Impl;
using Utilities;
using Utilities.Factories;

namespace Model.Facades
{
    public static class WebRequestFacade
    {
        /// <summary>
        /// Creates the web request.
        /// </summary>
        /// <param name="address">The Uri address.</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <returns>The web request.</returns>
        private static WebRequest CreateWebRequest(string address, string method, Dictionary<string, string> additionalHeaders = null)
        {
            if (address == null) { throw new ArgumentNullException("address"); }
            if (method == null) { throw new ArgumentNullException("method"); }

            Logger.WriteTrace("Creating HttpWebRequest for {0}", address);
            WebRequest request = HttpWebRequest.Create(new Uri(address));
            request.Method = method;
            request.ContentLength = 0;
            if (additionalHeaders != null)
            {
                foreach (string key in additionalHeaders.Keys)
                {
                    Logger.WriteTrace("*** Adding Headers[{0}] = '{1}'", key, additionalHeaders[key]);
                    if (key == "Content-Type")
                        request.ContentType = additionalHeaders[key];
                    else
                    request.Headers[key] = additionalHeaders[key];
                }
            }

            return request;
        }

        /// <summary>
        /// Creates the web request and get the response which is then serialized into the specified type.
        /// </summary>
        /// <typeparam name="T">The type of object to be returned by the GetResponse() call.</typeparam>
        /// <param name="address">The Uri address.</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected HTTP status codes.  By default only 200 OK is expected.</param>
        /// <returns>The response object(s).</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if the HTTP status code returned wasn't in the expected list of status codes.</exception>
        public static T CreateWebRequestAndGetResponse<T>(string address, string method,
            Dictionary<string, string> additionalHeaders = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            WebRequest request = CreateWebRequest(address, method, additionalHeaders);
            T responseValue;
            Logger.WriteTrace("request.GetResponse()");

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    Logger.WriteDebug("GetResponse() got back Status Code: {0}", response.StatusCode);

                    if (expectedStatusCodes == null) { expectedStatusCodes = new List<HttpStatusCode>() { HttpStatusCode.OK }; }

                    if (!expectedStatusCodes.Contains(response.StatusCode))
                    {
                        throw WebExceptionFactory.Create((int)response.StatusCode);
                    }

                    var encoding = Encoding.UTF8;
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream, encoding);
                    string jsonText = reader.ReadToEnd();
                    responseValue = JsonConvert.DeserializeObject<T>(jsonText);
                }
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e);
            }

            return responseValue;
        }

        /// <summary>
        /// Creates the web request and gets the response.
        /// </summary>
        /// <typeparam name="T">The type of object to be returned by the GetResponse() call.</typeparam>
        /// <param name="address">The Uri address.</param>
        /// <param name="method">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <returns>The response (NOTE: the caller should call Dispose() on this HttpWebRequest).</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if an error occurred.</exception>
        public static HttpWebResponse CreateWebRequestAndGetResponse(string address, string method, Dictionary<string, string> additionalHeaders = null)
        {
            WebRequest request = CreateWebRequest(address, method, additionalHeaders);

            Logger.WriteTrace("request.GetResponse()");

            try
            {
                Logger.WriteDebug("*** Headers = '{0}'", request.Headers);
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                Logger.WriteDebug("GetResponse() got back Status Code: {0}", response.StatusCode);
                return response;
            }
            catch (WebException e)
            {
                throw WebExceptionConverter.Convert(e);
            }
        }

        /// <summary>
        /// Creates the web request, gets the response and returns WebResponseFacade object.
        /// </summary>
        /// <param name="address">The Uri address.</param>
        /// <param name="httpMethod">The method (GET, POST...).</param>
        /// <param name="additionalHeaders">(optional) Additional headers to add to the request.</param>
        /// <param name="requestBody">(optional) json for POST</param>
        /// <returns>The WebResponseFacade.</returns>
        /// <exception cref="WebException">A WebException (or a sub-exception type) if an error occurred.</exception>
        public static WebResponseFacade GetWebResponseFacade(string address, string httpMethod = "GET", Dictionary<string, string> additionalHeaders = null,
            object requestBody = null)
        {
            var fetchRequest = CreateWebRequest(address, httpMethod, additionalHeaders);
            UTF8Encoding encoding = new UTF8Encoding();
            if (!ReferenceEquals(requestBody, null))
            {
                string jsonBody = JsonConvert.SerializeObject(requestBody);
                byte[] data = encoding.GetBytes(jsonBody);
                fetchRequest.ContentLength = data.Length;
                using (Stream requestStream = fetchRequest.GetRequestStream())
                {
                    requestStream.Write(data, 0, data.Length);
                }
            }
            else
            {
                fetchRequest.ContentLength = 0;
            }
            HttpWebResponse objResponse = null;
            try
            {
                objResponse = fetchRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    throw;
                }
                objResponse = (HttpWebResponse)e.Response;
            }
            string jsonText = String.Empty;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var key in objResponse.Headers.AllKeys)
            {
                headers.Add(key, objResponse.Headers[key]);
            }
            using (Stream responseStream = objResponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, encoding);
                jsonText = reader.ReadToEnd();
            }
            return new WebResponseFacade(objResponse.StatusCode, jsonText, headers);
        }
    }
}

