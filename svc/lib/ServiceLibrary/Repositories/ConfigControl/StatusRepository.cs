using ServiceLibrary.Helpers;
using ServiceLibrary.LocalEventLog;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [RoutePrefix("status")]
    public class StatusRepository : IStatusRepository
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        public StatusRepository()
            : this(new HttpClientProvider())
        {
        }

        public StatusRepository(IHttpClientProvider hcp)
        {
            _httpClientProvider = hcp;
        }

        [Route("")]
        public async Task<bool> GetStatus()
        {
            bool status = false;

            try
            {
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(uri);
                    http.DefaultRequestHeaders.Accept.Clear();

                    HttpResponseMessage response = await http.GetAsync("status");

                    response.EnsureSuccessStatusCode();

                    status = true;
                }

            }
            catch (Exception ex)
            {
                LocalLog.Log.LogError(string.Format("Problem with ConfigControl Status service: {0}", ex.Message));
            }

            return status;
        }
    }
}
