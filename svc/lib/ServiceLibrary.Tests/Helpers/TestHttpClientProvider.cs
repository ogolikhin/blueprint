using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class TestHttpClientProvider : HttpClientHandler, IHttpClientProvider
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
        private readonly Func<HttpClient, HttpClient> _initHttpClient;

        public TestHttpClientProvider(Func<HttpRequestMessage, HttpResponseMessage> handler,
            Func<HttpClient, HttpClient> initHttpClient = null)
        {
            _handler = handler;
            _initHttpClient = initHttpClient;
        }

        public HttpClient Create()
        {
            var http = new HttpClient(this);

            if (_initHttpClient != null)
            {
                _initHttpClient(http);
            }

            return http;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Run(() => _handler(request), cancellationToken);
        }
    }
}
