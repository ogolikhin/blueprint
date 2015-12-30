using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;
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
        private readonly ILocalLog _localLog;

        public StatusRepository()
            : this(new HttpClientProvider(), new LocalFileLog())
        {
        }

        public StatusRepository(IHttpClientProvider hcp, ILocalLog localLog)
        {
            _httpClientProvider = hcp;
            _localLog = localLog;
        }

        [Route("")]
        public async Task<bool> GetStatus()
        {
            bool status = false;

            try
            {
                var uri = ConfigurationManager.AppSettings["ConfigControl"];
                if (string.IsNullOrWhiteSpace(uri)) throw new ApplicationException("Application setting not set: ConfigControl");
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
                _localLog.LogError(string.Format("Problem with ConfigControl Status service: {0}", ex.Message));
            }

            return status;
        }
    }
}
