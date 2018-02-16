// *************************************************************************************
// ***** Any changes to this file need to be replicated in the                     *****
// ***** ServiceLibrary project in the Bluprint and BluePrint-Current repositories *****
// *************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;

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

        /// <summary>
        /// Creates HttpClient object whose HttpMessageHandler has a custom ServerCertificateValidationCallback
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="ignoreSSLCertErrors"></param>
        /// <returns></returns>
        HttpClient CreateWithCustomCertificateValidation(Uri baseAddress, bool ignoreSSLCertErrors, int connectionTimeout);

        /// <summary>
        /// Checks if the HttpClient is configured to ignore Certificate Errors
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <returns></returns>
        bool HttpClientIgnoresCertificateErrors(Uri baseAddress);

        /// <summary>
        /// Updates the HttpClient configuration of ignoring Certificate Errors
        /// </summary>
        /// <param name="baseAddress"></param>
        /// <param name="ignoreSSLCertErrors"></param>
        bool UpdateHttpClient(Uri baseAddress, bool ignoreSSLCertErrors);
    }

    public class HttpClientProvider : IHttpClientProvider
    {
        private static readonly ConcurrentDictionary<Uri, HttpClient> HttpClients = new ConcurrentDictionary<Uri, HttpClient>();

        private static readonly ConcurrentDictionary<Uri, bool> HttpClientsWhoIgnoreCertificateErrors = new ConcurrentDictionary<Uri, bool>();

        public HttpClient Create(Uri baseAddress)
        {
            return HttpClients.GetOrAdd(baseAddress, CreateInternal);
        }

        public HttpClient CreateWithCustomCertificateValidation(Uri baseAddress, bool ignoreSSLCertErrors, int connectionTimeout)
        {
            HttpClient httpClient;

            if (ignoreSSLCertErrors)
            {
                httpClient = HttpClients.GetOrAdd(baseAddress, CreateInternalWhoIgnoresCertificateErrors);
                httpClient.Timeout = TimeSpan.FromSeconds(connectionTimeout);
                return httpClient;
            }

            httpClient = HttpClients.GetOrAdd(baseAddress, CreateInternal);
            httpClient.Timeout = TimeSpan.FromSeconds(connectionTimeout);
            return httpClient;
        }

        public bool HttpClientIgnoresCertificateErrors(Uri baseAddress)
        {
            if (HttpClientsWhoIgnoreCertificateErrors.ContainsKey(baseAddress))
            {
                return true;
            }

            return false;
        }

        public bool UpdateHttpClient(Uri baseAddress, bool ignoreSSLCertErrors)
        {
            bool addUrlToInternalList = !HttpClientsWhoIgnoreCertificateErrors.ContainsKey(baseAddress) && ignoreSSLCertErrors;
            bool removeUrlFromInternalList = HttpClientsWhoIgnoreCertificateErrors.ContainsKey(baseAddress) && !ignoreSSLCertErrors;

            if (addUrlToInternalList)
            {
                HttpClientsWhoIgnoreCertificateErrors.AddIfNotListed(new KeyValuePair<Uri, bool>(baseAddress, true));
                return true;
            }

            if (removeUrlFromInternalList)
            {
                bool urlSuccessfullyRemoved;
                HttpClientsWhoIgnoreCertificateErrors.TryRemove(baseAddress, out urlSuccessfullyRemoved);
                return urlSuccessfullyRemoved;
            }

            // No changes required to internal dictionary - everything is uptodate
            return true;
        }

        private HttpClient CreateInternal(Uri baseAddress)
        {
            var result = new HttpClient { BaseAddress = baseAddress };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }

        private HttpClient CreateInternalWhoIgnoresCertificateErrors(Uri baseAddress)
        {
            var httpClientHandler = new WebRequestHandler();
            httpClientHandler.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }

                var senderUrl = ((HttpWebRequest)sender).Address;
                if (HttpClientsWhoIgnoreCertificateErrors.ContainsKey(senderUrl))
                {
                    return true;
                }

                return false;
            };

            HttpClientsWhoIgnoreCertificateErrors.AddIfNotListed(new KeyValuePair<Uri, bool>(baseAddress, true));

            var result = new HttpClient(httpClientHandler) { BaseAddress = baseAddress };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }
    }
}
