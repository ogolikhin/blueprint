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

        private readonly Uri _serviceUri;

        public ServiceDependencyStatusRepository(Uri serviceUri, string name)
        {
            _serviceUri = serviceUri;
            Name = name;
        }

        public async Task<string> GetStatus(int timeout)
        {
            //Note: Getting an HttpClient from HttpClientProvider doesn't work, because we
            //won't be able to change the timeout if the HttpClient has been used before.

            var webRequest = WebRequest.CreateHttp(new Uri(_serviceUri, "status/upcheck"));
            webRequest.Timeout = timeout;
            HttpWebResponse result = (HttpWebResponse)await webRequest.GetResponseAsync();
           
            return ((int)result.StatusCode).ToString();
        }
    }
}
