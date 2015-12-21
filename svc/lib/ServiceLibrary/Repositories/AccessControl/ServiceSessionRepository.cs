using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public class ServiceSessionRepository : ISessionRepository
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        public ServiceSessionRepository()
            : this(new HttpClientProvider())
        {
        }

        internal ServiceSessionRepository(IHttpClientProvider hcp)
        {
            _httpClientProvider = hcp;
        }

        // Wrapper call for Access Control Put method
        public async Task GetAccessAsync(HttpRequestMessage request, string op, int aid)
        {
            var uri = ConfigurationManager.AppSettings["AccessControl"] + op + "/" + aid.ToString();
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(uri);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Add("Session-Token", GetHeaderSessionToken(request));
                var result = await http.PutAsync("sessions", null);
                result.EnsureSuccessStatusCode();
            }
        }
        private string GetHeaderSessionToken(HttpRequestMessage request)
        {
            if (request.Headers.Contains("Session-Token") == false)
                throw new ArgumentNullException();
            return request.Headers.GetValues("Session-Token").FirstOrDefault();
        }
    }
}
