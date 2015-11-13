using System.Net.Http;

namespace AdminStore.Helpers
{
    /// <summary>
    /// Provides HttpClient. Hook for unit testing.
    /// </summary>
    /// <example> 
    /// This sample shows how to create HttpClient for unit tests
    /// <code>
    /// class FakeHttpClientProvider : IHttpClientProvider
    /// {
    ///     public HttpClient CreateHttpClient()
    ///     {
    ///         return new HttpClient(new FakeResponseHandler());
    ///     }
    /// }
    /// 
    ///public class FakeResponseHandler : DelegatingHandler
    ///{
    ///     protected async override Task SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    ///     {
    ///         return await Task.Run(() =>
    ///              {
    ///                  var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
    ///                  httpResponseMessage.Headers.Add("Session-Token", Guid.NewGuid().ToString());
    ///                  return httpResponseMessage;
    ///              },  cancellationToken);
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IHttpClientProvider
    {
        /// <summary>
        /// Creates HttpClient object
        /// </summary>

        HttpClient CreateHttpClient();
    }

    public class HttpClientProvider : IHttpClientProvider
    {
        public HttpClient CreateHttpClient()
        {
            return new HttpClient();
        }
    }
}