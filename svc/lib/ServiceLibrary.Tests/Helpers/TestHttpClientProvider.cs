using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class TestHttpClientProvider : HttpClientHandler, IHttpClientProvider
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public TestHttpClientProvider(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        public HttpClient Create(Uri baseAddress)
        {
            var result = new HttpClient(this) { BaseAddress = baseAddress };
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }

        public HttpClient CreateWithCustomCertificateValidation(Uri baseAddress, bool ignoreSSLCertErrors, int connectionTimeout)
        {
            return Create(baseAddress);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Run(() => _handler(request), cancellationToken);
        }
    }
}
