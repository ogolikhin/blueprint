using System;
using System.Net.Http;
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

        public HttpClient Create()
        {
            return new HttpClient(this);
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return await Task.Run(() => _handler(request), cancellationToken);
        }
    }
}
