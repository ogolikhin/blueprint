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
        /// <param name="connectionTimeout"></param>
        /// <returns></returns>
        HttpClient CreateWithCustomCertificateValidation(Uri baseAddress, bool ignoreSSLCertErrors, int connectionTimeout);
    }

    public class HttpClientProvider : IHttpClientProvider
    {
        private static readonly ConcurrentDictionary<Uri, HttpClient> HttpClients = new ConcurrentDictionary<Uri, HttpClient>();

        private static readonly ConcurrentDictionary<Uri, HttpClient> HttpClientsWhoIgnoreCertificateErrors = new ConcurrentDictionary<Uri, HttpClient>();

        public HttpClient Create(Uri baseAddress)
        {
            return HttpClients.GetOrAdd(baseAddress, CreateInternal);
        }

        public HttpClient CreateWithCustomCertificateValidation(Uri baseAddress, bool ignoreSSLCertErrors, int connectionTimeout)
        {
            if (ignoreSSLCertErrors)
            {
                return HttpClientsWhoIgnoreCertificateErrors.GetOrAdd(baseAddress, CreateInternalIgnoresCertificateErrors(baseAddress, connectionTimeout));
            }

            return HttpClients.GetOrAdd(baseAddress, CreateInternalWithTimeout(baseAddress, connectionTimeout));
        }

        private HttpClient CreateInternal(Uri baseAddress)
        {
            var result = new HttpClient { BaseAddress = baseAddress };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }

        private HttpClient CreateInternalWithTimeout(Uri baseAddress, int connectionTimeout)
        {
            var result = new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = TimeSpan.FromSeconds(connectionTimeout)
            };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }

        private HttpClient CreateInternalIgnoresCertificateErrors(Uri baseAddress, int connectionTimeout)
        {
            var httpClientHandler = new WebRequestHandler();
            httpClientHandler.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
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

            return CreateInternalWithTimeout(baseAddress, connectionTimeout);
        }
    }
}
