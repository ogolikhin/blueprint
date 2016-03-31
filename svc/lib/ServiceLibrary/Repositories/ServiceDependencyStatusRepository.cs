using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    public class ServiceDependencyStatusRepository : IStatusRepository
    {
        public string Name { get; set; }
        public string AccessInfo { get; set; }

        private readonly Uri _requestUri;

        public ServiceDependencyStatusRepository(Uri serviceUri, string name)
        {
            _requestUri = new Uri(serviceUri, "status/upcheck");
            Name = name;
            AccessInfo = _requestUri.ToString();
        }

        public async Task<string> GetStatus(int timeout)
        {
            //Note: Getting an HttpClient from HttpClientProvider doesn't work, because we
            //won't be able to change the timeout if the HttpClient has been used before.
            
            var webRequest = WebRequest.CreateHttp(_requestUri);
            webRequest.Timeout = timeout;

            using (HttpWebResponse result = (HttpWebResponse) await webRequest.GetResponseAsync())
            {
                return ((int)result.StatusCode).ToString();
            }      
        }
    }
}
