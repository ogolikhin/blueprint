using System;
using System.Net;
using Logging;
using System.Text;
using Model.Impl;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Model.Facades
{
    public class WebResponseFacade
    {
        /// <summary>
        /// Creates the web response facade.
        /// </summary>
        /// <param name="responseStatus">HttpStatus of the web response</param>
        /// <param name="responseJson">String with encoded JSON or error message.</param>
        /// <param name="headers">Headers of the web response.</param>
        /// <returns>The web response facade.</returns>
        public WebResponseFacade(HttpStatusCode responseStatus, string responseJson, Dictionary<string, string> headers)
        {
            this._statusCode = responseStatus;
            this._response = responseJson;
            this._headers = headers;
        }

        /// <summary>
        /// Deserialize JSON into Blueprint object.
        /// </summary>
        /// <typeparam name="T">The type of object to be returned.</typeparam>
        /// <returns>Deserialized object(s).</returns>
        public T GetBlueprintObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(this._response);
        }

        /// <summary>
        /// Returns responses header's value by name.
        /// </summary>
        /// <param name="keyName">name of the header's key</param>
        /// <returns>The header's value.</returns>
        public string GetHeaderValue(string keyName)
        {
            return this._headers[keyName];
        }

        /// <summary>
        /// Returns HttpStatusCode of the response.
        /// </summary>
        /// <returns>HttpStatusCode of the response.</returns>
        public HttpStatusCode StatusCode
        {
            get { return this._statusCode; }
        }

        /// <summary>
        /// Returns response body as a string without decoding. Can be use to read error message or when response isn't encoded
        /// </summary>
        /// <returns>Response body.</returns>
        public string ResponseString
        {
            get { return _response; }
        }

        private readonly HttpStatusCode _statusCode;
        private readonly string _response;
        private readonly Dictionary<string, string> _headers;
    }
}
