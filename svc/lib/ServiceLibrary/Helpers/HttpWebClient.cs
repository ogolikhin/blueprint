using System;
using System.Net;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class HttpWebClient : IHttpWebClient
    {
        private readonly Uri _baseUri;
        private readonly string _sessionToken;
        private readonly int _timeout;

        public HttpWebClient(Uri baseUri, string sessionToken, int timeout = ServiceConstants.DefaultRequestTimeout)
        {
            _baseUri = baseUri;
            _sessionToken = sessionToken;
            _timeout = timeout;
        }

        public HttpWebRequest CreateHttpWebRequest(string requestAddress, string method)
        {
            var requestUri = new Uri(_baseUri, requestAddress);
            var request = WebRequest.CreateHttp(requestUri);
            request.Headers[ServiceConstants.BlueprintSessionTokenKey] = _sessionToken;
            request.Method = method;
            request.Timeout = _timeout;

            return request;
        }

        public async Task<HttpWebResponse> GetHttpWebResponseAsync(HttpWebRequest request)
        {
            HttpWebResponse response;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    // HttpWebRequest that gets created has ConnectionLimit = 2 by default when calling from remote host.
                    var message = $"Timeout exception occured. Current connections: {request.ServicePoint.CurrentConnections}, Connection limit: {request.ServicePoint.ConnectionLimit}\n{ex}";
                    throw new Exception(message);
                }

                var exceptionResponse = ex.Response as HttpWebResponse;
                if (exceptionResponse != null)
                {
                    response = exceptionResponse;
                }
                else
                {
                    throw;
                }
            }

            return response;
        }
    }
}
