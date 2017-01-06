using System;
using System.Net;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public interface IHttpWebClient
    {
        HttpWebRequest CreateHttpWebRequest(Uri requestUri, string method, string sessionToken, int timeout);

        Task<HttpWebResponse> GetHttpWebResponseAsync(HttpWebRequest request);
    }
}
