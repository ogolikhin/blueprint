using System.Net;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public interface IHttpWebClient
    {
        HttpWebRequest CreateHttpWebRequest(string requestAddress, string method);

        Task<HttpWebResponse> GetHttpWebResponseAsync(HttpWebRequest request);
    }
}
