// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************

using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<Uri, HttpClient> HttpClients = new ConcurrentDictionary<Uri, HttpClient>();

        public HttpClient Create(Uri baseAddress)
        {
            return HttpClients.GetOrAdd(baseAddress, CreateInternal);
        }

        private HttpClient CreateInternal(Uri baseAddress)
        {
            var result = new HttpClient { BaseAddress = baseAddress };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }
    }
}
