using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class HttpWebClient : IHttpWebClient
    {
        public HttpWebRequest CreateHttpWebRequest(Uri requestUri, string method, string sessionToken, int timeout)
        {
            var request = WebRequest.CreateHttp(requestUri);
            request.Headers[ServiceConstants.BlueprintSessionTokenKey] = sessionToken;
            request.Method = method;
            request.Timeout = timeout;

            return request;
        }

        public async Task<HttpWebResponse> GetHttpWebResponseAsync(HttpWebRequest request)
        {
            HttpWebResponse response = null;

            try
            {
                response = await request.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    // HttpWebRequest that gets created has ConnectionLimit = 2 by default when calling from remote host.
                    throw new Exception
                    (
                        string.Format
                        (
                            "Timeout exception occured. Current connections: {0}, Connection limit: {1}\n{2}",
                            request.ServicePoint.CurrentConnections,
                            request.ServicePoint.ConnectionLimit,
                            ex
                        )
                    );
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
