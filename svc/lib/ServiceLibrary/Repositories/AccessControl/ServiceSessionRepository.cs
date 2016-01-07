using ServiceLibrary.Helpers;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories.ConfigControl
{
    public class ServiceSessionRepository : ISessionRepository
    {
        private const string BlueprintSessionToken = "Session-Token";
        private const string BlueprintSessionCookie = "BLUEPRINT_SESSION_TOKEN";
        private const string AccessControl = "AccessControl";

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
        public async Task GetAccessAsync(HttpRequestMessage request)
        {
            var uri = ConfigurationManager.AppSettings[AccessControl];
            using (var http = _httpClientProvider.Create())
            {
                http.BaseAddress = new Uri(uri);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Add(BlueprintSessionToken, GetHeaderSessionToken(request));
                var result = await http.PutAsync("sessions", null);
                result.EnsureSuccessStatusCode();
            }
        }

        private string GetHeaderSessionToken(HttpRequestMessage request)
        {
            if (request.Headers.Contains(BlueprintSessionToken) == false)
            {
                if (request.Method != HttpMethod.Get)
                {
                    throw new ArgumentNullException();
                }
                var sessionTokenCookie = request.Headers.GetCookies("BLUEPRINT_SESSION_TOKEN").FirstOrDefault();
                if (sessionTokenCookie == null)
                {
                    throw new ArgumentNullException();
                }
                var value = sessionTokenCookie[BlueprintSessionCookie].Value;


                return value;
            }
            return request.Headers.GetValues(BlueprintSessionToken).FirstOrDefault();
        }
    }
}
