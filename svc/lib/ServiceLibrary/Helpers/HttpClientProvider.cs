// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ServiceLibrary.Helpers
{
    /// <summary>
    /// Provides HttpClient. Hook for unit testing.
    /// </summary>
    /// <example>
    /// This sample shows how to create HttpClient for unit tests
    /// <code>
    /// var httpClientProvider = new TestHttpClientProvider(request =>
    /// {
    ///     var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
    ///     httpResponseMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
    ///     return httpResponseMessage;
    /// });
    /// </code>
    /// </example>
    public interface IHttpClientProvider
    {
        /// <summary>
        /// Creates HttpClient object
        /// </summary>
        /// <param name="baseAddress"></param>
        HttpClient Create(Uri baseAddress);
    }

    public class HttpClientProvider : IHttpClientProvider
    {
        private static readonly IDictionary<Uri, HttpClient> HttpClients = new Dictionary<Uri, HttpClient>();

        public HttpClient Create(Uri baseAddress)
        {
            HttpClient result;
            if (!HttpClients.TryGetValue(baseAddress, out result))
            {
                result = new HttpClient { BaseAddress = baseAddress };
                result.DefaultRequestHeaders.Accept.Clear();
                result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpClients[baseAddress] = result;
            }
            return result;
        }
    }
}
